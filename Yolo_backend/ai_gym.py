# Ultralytics YOLO 🚀, AGPL-3.0 license
import cv2
import logging
from ultralytics.solutions.solutions import BaseSolution
from ultralytics.utils.plotting import Annotator
import traceback

# 尝试导入所有运动类型的跟踪器类
try:
    from squat import SquatTracker
    from deadlift import DeadliftTracker
    from pushup import PushupTracker
    from abworkout import AbworkoutTracker
    from benchpress import BenchPressTracker
except ImportError as e:
    logging.error(f"Failed to import tracker modules: {e}")

# 设置日志记录
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class AIGym(BaseSolution):
    def __init__(self, kpts_to_check, line_thickness=2, pose_type="pushup",
                 **kwargs):
        # Check if the model name ends with '-pose'
        if "model" in kwargs and "-pose" not in kwargs["model"]:
            kwargs["model"] = "yolov8m-pose.pt"
        elif "model" not in kwargs:
            kwargs["model"] = "yolov8m-pose.pt"

        """初始化 AIGym，以便使用姿态估计和预定义角度监控锻炼。"""
        self.im0 = None
        self.tf = line_thickness
        self.keypoints = None
        self.threshold = 0.001
        self.angle = None
        self.count = None
        self.stage = None
        self.pose_type = pose_type
        self.annotator = None
        # self.env_check = check_imshow(warn=True)
        self.fps = 30

        super().__init__(**kwargs)
        self.count = 0
        self.angle = 0
        self.stage = "-"
        # 从配置中提取详细信息以供后续使用
        self.initial_stage = None
        self.poseup_angle = float(self.CFG["up_angle"])  # 预定义的"向上"姿态角度
        self.posedown_angle = float(self.CFG["down_angle"])  # 预定义的"向下"姿态角度
        self.kpts = kpts_to_check  # 用户选择的用于锻炼的关键点存储，以供后续使用
        self.lw = line_thickness  # 确保这里赋值

        # 实例化跟踪器
        if pose_type == "squat":
            self.tracker = SquatTracker()  # 实例化 SquatTracker 类
        elif pose_type == "deadlift":
            self.tracker = DeadliftTracker()  # 实例化 DeadliftTracker 类
        elif pose_type == "pushup":
            self.tracker = PushupTracker()  # 实例化 PushupTracker
        elif pose_type == "abworkout":
            self.tracker = AbworkoutTracker()  # 实例化 AbworkoutTracker 类
        elif pose_type == "benchpress":
            self.tracker = BenchPressTracker()  # 实例化 BenchPressTracker 类
        else:
            logger.warning(f"Unsupported pose type: {pose_type}. Using default tracker.")
            self.tracker = PushupTracker()  # 默认使用 PushupTracker

        # 从跟踪器获取相关属性
        self._get_state = self.tracker._get_state
        self.state_tracker = self.tracker.state_tracker
        self._update_state_sequence = self.tracker._update_state_sequence
        self.thresholds = self.tracker.thresholds
        self.FEEDBACK_ID_MAP = self.tracker.FEEDBACK_ID_MAP  # 获取反馈映射

    def monitor(self, im0):
        # 提取跟踪数据
        try:
            logger.debug("开始YOLO模型跟踪处理...")
            # 克隆或复制帧以避免资源冲突
            frame_copy = im0.copy()
            logger.debug("帧复制完成")
            tracks = self.model.track(source=frame_copy, persist=True, classes=self.CFG["classes"])[0]
            logger.debug("YOLO模型跟踪完成")
        except Exception as e:
            error_msg = f"Error tracking objects: {e}"
            logger.error(error_msg)
            logger.error(traceback.format_exc())
            return im0, 0  # 返回原始图像和计数0
        
        # 添加空结果检查
        if not tracks or len(tracks) == 0:
            logger.info("No tracks found in the frame")
            return im0, 0  # 返回原始图像和计数0
            
        # 只处理第一个人物（索引为0）
        if tracks.boxes.id is not None:
            # 初始化注释对象
            self.annotator = Annotator(im0, line_width=self.lw)

            # 只处理第一个检测到的人物
            k = tracks.keypoints.data[0]  # 只获取第一个人的关键点数据

            # 获取关键点并估计角度
            try:
                kpts = [k[int(self.kpts[i])].cpu() for i in range(3)]
                self.angle = self.annotator.estimate_pose_angle(*kpts)
                im0 = self.annotator.draw_specific_points(k, self.kpts, radius=self.lw * 3)
            except Exception as e:
                logger.error(f"Error processing keypoints: {e}")
                logger.error(traceback.format_exc())

            # 使用跟踪器处理特定运动类型

            if self.pose_type in {"deadlift", "pushup", "squat", "abworkout", "benchpress"}:
                # 调用 track 方法，传入一个只有一个元素的列表 [self.count]
                try:
                    # 创建一个包含单个元素的列表传递给tracker
                    count_list = [self.count]
                    im0 = self.tracker.track(k, im0, 0, count_list)
                    # 更新计数器
                    self.count = count_list[0]
                except Exception as e:
                    logger.error(f"Error tracking {self.pose_type}: {e}")
                    logger.error(traceback.format_exc())


        if hasattr(self.annotator, 'kpts'):
            self.annotator.kpts(k, shape=(640,640), radius=1, kpt_line=True)
            
        return im0, self.count  # 返回处理后的图像和计数值


"""
0    nose    鼻子
1    left_eye    左眼
2    right_eye    右眼
3    left_ear    左耳
4    right_ear    右耳
5    left_shoulder    左肩
6    right_shoulder    右肩
7    left_elbow    左肘
8    right_elbow    右肘
9    left_wrist    左腕
10    right_wrist    右腕
11    left_hip    左髋
12    right_hip    右髋
13    left_knee    左膝
14    right_knee    右膝
15    left_ankle    左踝
16    right_ankle    右踝
"""