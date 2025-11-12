import numpy as np
from function import draw_text, calculate_angle, _show_feedback, draw_dotted_line, find_angle
import cv2

class DeadliftTracker:
    def __init__(self):
        """
        初始化深蹲状态追踪器。
        """
        # 初始化状态跟踪字典
        self.state_tracker = {
            'state_seq': [],
            'INACTIVE_TIME': 0.0,
            'INACTIVE_TIME_FRONT': 0.0,
            'DISPLAY_TEXT': np.full((4,), False),
            'COUNT_FRAMES': np.zeros((4,), dtype=np.int64),
            'LOWER_HIPS': False,
            'INCORRECT_POSTURE': False,
            'prev_state': None,
            'curr_state': None,
            'DEADLIFT_COUNT': 0,
            'IMPROPER_DEADLIFT': 0
        }

        # 设置反馈信息映射
        self.FEEDBACK_ID_MAP = {
            0: ('BACK TOO ARCHED', 215, (0, 153, 255)),  # 背部过度弯曲
            1: ('KEEN TOO LOW', 215, (0, 153, 255)),
            2: ('ARM BENDING', 125, (255, 80, 80))  # 手臂弯曲
        }

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
            'SHOULDER_HIP_KNEE': {
                'NORMAL': (15, 110),  # 初始阶段
                'TRANS': (115, 130),  # 过渡阶段
                'PASS': (135, 180)  # 完成阶段
            },
            'HIP_THRESH': 70,  # 髋关节角度阈值
            'KNEE_THRESH': 45,  # 膝关节角度范围
            'ARM_THRESH': 140,
            'INACTIVE_THRESH': 15.0,  # 静止时间阈值 (秒)
            'CNT_FRAME_THRESH': 50  # 维持帧数阈值
        }
        return thresholds


    def _get_state(self, shoulder_hip_knee_angle):
        angle = None
        # 根据肩-髋-膝夹角判断阶段
        if self.thresholds['SHOULDER_HIP_KNEE']['NORMAL'][0] <= shoulder_hip_knee_angle <= \
                self.thresholds['SHOULDER_HIP_KNEE']['NORMAL'][1]:
            angle = 1  # 下放阶段，肩-髋-膝角度在正常范围
        elif self.thresholds['SHOULDER_HIP_KNEE']['TRANS'][0] <= shoulder_hip_knee_angle <= \
                self.thresholds['SHOULDER_HIP_KNEE']['TRANS'][1]:
            angle = 2  # 过渡阶段，肩-髋-膝角度处于过渡范围
        elif self.thresholds['SHOULDER_HIP_KNEE']['PASS'][0] <= shoulder_hip_knee_angle <= \
                self.thresholds['SHOULDER_HIP_KNEE']['PASS'][1]:
            angle = 3  # 完成阶段，肩-髋-膝角度达到最大值

        return f's{angle}' if angle else None  # 返回对应的状态，若无有效状态则返回 None

    def _update_state_sequence(self, state):
        """
        更新状态序列，根据新的状态添加到状态序列中。

        参数:
            state: 当前状态（如 's1', 's2' 等）
        """
        if state == 's2':
            # 添加状态 's2' 到状态序列
            if (('s3' not in self.state_tracker['state_seq']) and (self.state_tracker['state_seq'].count('s2')) == 0) or \
                    (('s3' in self.state_tracker['state_seq']) and (
                            self.state_tracker['state_seq'].count('s2') == 1)):
                self.state_tracker['state_seq'].append(state)

        elif state == 's3':
            # 添加状态 's3' 到状态序列
            if (state not in self.state_tracker['state_seq']) and 's2' in self.state_tracker['state_seq']:
                self.state_tracker['state_seq'].append(state)

    def track(self, k, im0, ind, count):
        """
        处理硬拉检测和计数的主逻辑，包含状态跟踪、姿势检查和反馈显示。

        参数:
        - k: 姿势关键点
        - im0: 当前帧图像
        - ind: 当前处理对象的索引（对于单人模式，始终为0）
        - count: 计数器列表（对于单人模式，只包含一个元素）
        """
        frame_height, frame_width, _ = im0.shape
        hip_knee_angle = calculate_angle(k[12].cpu(), k[14].cpu(), reference_direction='horizontal')  # 髋膝夹角，参考水平
        shoulder_hip_angle = calculate_angle(k[6].cpu(), k[12].cpu(), reference_direction='horizontal')  # 肩膝夹角，参考水平
        shoulder_hip_knee_angle = shoulder_hip_angle + hip_knee_angle
        
        draw_text(
            im0,
            "CORRECT: " + str(self.state_tracker['DEADLIFT_COUNT']),
            pos=(int(frame_width * 0.68), 130),
            text_color=(255, 255, 230),
            font_scale=0.7,
            text_color_bg=(18, 185, 0)
        )
        # 调试：在屏幕上显示angle值
        draw_text(
            im0,
            "ANGLE: " + str(round(shoulder_hip_knee_angle, 2)),
            pos=(int(frame_width * 0.68), 180),
            text_color=(255, 255, 255),
            font_scale=0.7,
            text_color_bg=(0, 0, 0)
        )
        # 定义髋关节、膝关节、肩关节的坐标
        right_knee = (int(k[14][0].cpu().item()), int(k[14][1].cpu().item()))
        right_hip = (int(k[12][0].cpu().item()), int(k[12][1].cpu().item()))
        # 设置虚线长度
        dotted_line_length = 60
        # 在膝关节、髋关节和肩关节上方绘制竖直虚线
        im0 = draw_dotted_line(im0, right_knee, right_knee[1] - dotted_line_length, right_knee[1], (255, 0, 0))
        im0 = draw_dotted_line(im0, right_hip, right_hip[1] - dotted_line_length, right_hip[1], (255, 0, 0))

        current_state = self._get_state(shoulder_hip_knee_angle)
        # 将当前状态存储
        self.state_tracker['curr_state'] = current_state
        # 更新姿态序列
        self._update_state_sequence(current_state)

        if current_state == 's1':  # 当前状态为 's1'，表示动作已达到正常的标准阶段（例如硬拉完全到位）
            # 检查状态序列长度为 3 且姿势不错误
            if len(self.state_tracker['state_seq']) == 3 and not self.state_tracker['INCORRECT_POSTURE']:
                # 状态序列为 [s1, s1, s1]，表示连续三个正常的标准阶段动作，并且姿势没有错误
                self.state_tracker['DEADLIFT_COUNT'] += 1  # 增加正确的硬拉计数
                count[0] += 1  # 更新传入的计数器列表中的第一个元素
            # 如果状态序列中包含过渡状态 's2' 且状态序列长度为 1
            elif 's2' in self.state_tracker['state_seq'] and len(self.state_tracker['state_seq']) == 1:
                # 如果在状态序列中发现过渡阶段 's2'，并且状态序列只包含这一个状态，表示动作没有完成
                self.state_tracker['IMPROPER_DEADLIFT'] += 1  # 增加不合格的硬拉计数
            # 如果姿势被标记为错误
            elif self.state_tracker['INCORRECT_POSTURE']:
                # 如果有不正确的姿势标志，表示此时的动作是错误的
                self.state_tracker['IMPROPER_DEADLIFT'] += 1  # 增加不合格的硬拉计数
            # 无论哪个条件被满足，都会重置状态序列和错误姿势标志
            self.state_tracker['state_seq'] = []  # 重置状态序列，准备记录下一次动作
            self.state_tracker['INCORRECT_POSTURE'] = False  # 重置错误姿势标志，表示此时动作不再被视为错误
        else:
            # 计算髋关节、膝关节、踝关节的角度
            shoulder_hip_vertical_angle = calculate_angle(k[6].cpu(), k[12].cpu(), reference_direction="vertical")
            knee_ankle_vertical_angle = calculate_angle(k[14].cpu(), k[16].cpu(), reference_direction="vertical")
            wrist_elbow_angle = calculate_angle(k[8].cpu(), k[10].cpu(), reference_direction="horizontal")
            elbow_shoulder_angle = calculate_angle(k[6].cpu(), k[8].cpu(), reference_direction="horizontal")
            arm_angle = wrist_elbow_angle + elbow_shoulder_angle
            # 检查髋关节垂直角度是否超过阈值
            if shoulder_hip_vertical_angle > self.thresholds['HIP_THRESH']:
                self.state_tracker['DISPLAY_TEXT'][0] = True  # 背部过低
            # 检查膝关节垂直角度是否大于过高阈值
            elif knee_ankle_vertical_angle > self.thresholds['KNEE_THRESH']:
                self.state_tracker['DISPLAY_TEXT'][1] = True
                self.state_tracker['INCORRECT_POSTURE'] = True
            # 检查踝关节垂直角度是否超过阈值
            if arm_angle < self.thresholds['ARM_THRESH']:
                self.state_tracker['DISPLAY_TEXT'][2] = True
                self.state_tracker['INCORRECT_POSTURE'] = True

            # 计数维持正确姿势的帧数
            self.state_tracker['COUNT_FRAMES'][self.state_tracker['DISPLAY_TEXT']] += 1

            # 显示反馈信息
            im0 = _show_feedback(im0, self.state_tracker['COUNT_FRAMES'], self.FEEDBACK_ID_MAP)
        return im0