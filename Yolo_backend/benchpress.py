import numpy as np
import cv2
from function import draw_text, calculate_angle, draw_dotted_line

class BenchPressTracker:
    def __init__(self):
        """
        初始化卧推状态追踪器。
        """
        # 初始化状态跟踪字典
        self.state_tracker = {
            'state_seq': [],
            'BENCHPRESS_COUNT': 0
        }

        # 设置反馈信息映射
        self.FEEDBACK_ID_MAP = {}

        # 获取阈值
        self.thresholds = self.get_thresholds_beginner()

    def get_thresholds(self):
        """
        获取阈值配置。

        返回:
            dict: 阈值字典。
        """
        return self.get_thresholds_beginner()

    def get_thresholds_beginner(self):
        """获取初学者模式的阈值设置"""
        thresholds = {
            'POSE_UP_ANGLE': 145.0,      # 上升阶段最大角度
            'POSE_DOWN_ANGLE': 85.0,     # 下降阶段最小角度
        }
        return thresholds


    def _get_state(self, angle):
        """
        根据角度判断动作状态。

        参数:
            angle (float): 当前角度值。

        返回:
            str: 姿势状态（如 's1', 's2' 等），如果不在范围内则返回 None。
        """
        # 简单的状态判断逻辑
        if angle > self.thresholds['POSE_UP_ANGLE']:
            return 's1'  # 上升阶段
        elif angle < self.thresholds['POSE_DOWN_ANGLE']:
            return 's2'  # 下降阶段
        else:
            return 's3'  # 过渡阶段

    def _update_state_sequence(self, state):
        """
        更新状态序列，根据新的状态添加到状态序列中。

        参数:
            state: 当前状态（如 's1', 's2' 等）
        """
        if state == 's2':
            # 添加状态 's2' 到状态序列
            if (('s3' not in self.state_tracker['state_seq']) and (self.state_tracker['state_seq'].count('s2') == 0)) or \
                    (('s3' in self.state_tracker['state_seq']) and (self.state_tracker['state_seq'].count('s2') == 1)):
                self.state_tracker['state_seq'].append(state)
        
        elif state == 's3':
            # 添加状态 's3' 到状态序列
            if (state not in self.state_tracker['state_seq']) and 's2' in self.state_tracker['state_seq']:
                self.state_tracker['state_seq'].append(state)

    def track(self, k, im0, ind, count):
        """
        处理卧推检测与计数的主逻辑。

        参数:
        - k: 姿势关键点
        - im0: 当前帧图像
        - ind: 当前处理对象的索引（对于单人模式，始终为0）
        - count: 计数器列表（对于单人模式，只包含一个元素）
        """
        frame_height, frame_width, _ = im0.shape
        
        # 计算左腕-左肘-左肩的角度 (使用正确的关键点索引)
        angle_left_wrist_elbow_shoulder = calculate_angle(k[9].cpu(), k[7].cpu(), k[5].cpu())
        
        # 绘制正确计数
        draw_text(
            im0,
            "CORRECT: " + str(self.state_tracker['BENCHPRESS_COUNT']),
            pos=(int(frame_width * 0.68), 130),
            text_color=(255, 255, 230),
            font_scale=0.7,
            text_color_bg=(18, 185, 0)
        )
        
        # 调试：在屏幕上显示angle值
        draw_text(
            im0,
            "ANGLE: " + str(round(angle_left_wrist_elbow_shoulder, 2)),
            pos=(int(frame_width * 0.68), 180),
            text_color=(255, 255, 255),
            font_scale=0.7,
            text_color_bg=(0, 0, 0)
        )

        # 获取当前状态
        current_state = self._get_state(angle_left_wrist_elbow_shoulder)
        self._update_state_sequence(current_state)

        # 卧推计数逻辑
        if current_state == 's1':
            if len(self.state_tracker['state_seq']) == 3:
                self.state_tracker['BENCHPRESS_COUNT'] += 1
                count[0] += 1  # 更新传入的计数器列表中的第一个元素
                
                # 重置状态序列
                self.state_tracker['state_seq'] = []

        return im0