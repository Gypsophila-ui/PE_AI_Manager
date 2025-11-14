import numpy as np
from function import draw_text, calculate_angle, _show_feedback, draw_dotted_line

class SquatTracker:
    def __init__(self):
        """
        初始化深蹲状态追踪器。
        """
        # 初始化状态跟踪字典
        self.state_tracker = {
            'state_seq': [],
            'DISPLAY_TEXT': np.full((4,), False),
            'COUNT_FRAMES': np.zeros((4,), dtype=np.int64),
            'INCORRECT_POSTURE': False,
            'curr_state': None,
            'SQUAT_COUNT': 0,
            'IMPROPER_SQUAT': 0
        }

        # 设置反馈信息映射
        self.FEEDBACK_ID_MAP = {
            0: ('LEAN BACKWARDS', 215, (0, 153, 255)),       # 后倾过多
            1: ('LEAN FORWARD', 215, (0, 153, 255)),          # 前倾过多
            2: ('KNEE OVER TOE', 170, (255, 80, 80)),         # 膝盖超过脚尖
            3: ('SQUAT TOO DEEP', 125, (255, 80, 80))         # 下蹲过深
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
        """
        获取初学者的阈值配置。
        """
        _ANGLE_HIP_KNEE_VERT = {
            'NORMAL': (0, 32),
            'TRANS': (35, 65),
            'PASS': (70, 95)
        }

        thresholds = {
            'HIP_KNEE_VERT': _ANGLE_HIP_KNEE_VERT,
            'HIP_THRESH': [10, 50],
            'ANKLE_THRESH': 45,
            'KNEE_THRESH': [50, 70, 95],
            'OFFSET_THRESH': 35.0,
            'INACTIVE_THRESH': 15.0,
            'CNT_FRAME_THRESH': 50
        }
        return thresholds


    def _get_state(self, knee_hip_angle):
        """
        根据膝盖和髋部夹角判断姿势状态。

        参数:
            knee_hip_angle (float): 膝盖和髋部之间的角度值。

        返回:
            str: 姿势状态（如 's1', 's2' 等），如果不在范围内则返回 None。
        """
        knee = None
        if self.thresholds['HIP_KNEE_VERT']['NORMAL'][0] <= knee_hip_angle <= self.thresholds['HIP_KNEE_VERT']['NORMAL'][1]:
            knee = 1  # 正常状态
        elif self.thresholds['HIP_KNEE_VERT']['TRANS'][0] <= knee_hip_angle <= self.thresholds['HIP_KNEE_VERT']['TRANS'][1]:
            knee = 2  # 过渡状态
        elif self.thresholds['HIP_KNEE_VERT']['PASS'][0] <= knee_hip_angle <= self.thresholds['HIP_KNEE_VERT']['PASS'][1]:
            knee = 3  # 完成状态

        return f's{knee}' if knee else None  # 返回对应的状态，若无有效状态则返回 None

    def _update_state_sequence(self, state):
        """
        更新状态序列，根据新的状态添加到状态序列中。

        参数:
            state (str): 当前状态（如 's2', 's3' 等）
        """
        if state == 's2':
            if (('s3' not in self.state_tracker['state_seq']) and (self.state_tracker['state_seq'].count('s2') == 0)) or \
                    (('s3' in self.state_tracker['state_seq']) and (self.state_tracker['state_seq'].count('s2') == 1)):
                self.state_tracker['state_seq'].append(state)

        elif state == 's3':
            if (state not in self.state_tracker['state_seq']) and 's2' in self.state_tracker['state_seq']:
                self.state_tracker['state_seq'].append(state)

    def track(self, k, im0, ind, count):
        """
        处理深蹲检测和计数的主逻辑，包含状态跟踪、姿势检查和反馈显示。

        参数:
        - k: 姿势关键点
        - im0: 当前帧图像
        - ind: 当前处理对象的索引（对于单人模式，始终为0）
        - count: 计数器列表（对于单人模式，只包含一个元素）
        """
        frame_height, frame_width, _ = im0.shape

        # 定义关节点坐标
        right_knee = (int(k[14][0].cpu().item()), int(k[14][1].cpu().item()))
        right_hip = (int(k[12][0].cpu().item()), int(k[12][1].cpu().item()))
        right_ankle = (int(k[16][0].cpu().item()), int(k[16][1].cpu().item()))

        # 绘制竖直虚线
        dotted_line_length = 60
        im0 = draw_dotted_line(im0, right_knee, right_knee[1] - dotted_line_length, right_knee[1], (255, 0, 0))
        im0 = draw_dotted_line(im0, right_hip, right_hip[1] - dotted_line_length, right_hip[1], (255, 0, 0))
        im0 = draw_dotted_line(im0, right_ankle, right_ankle[1] - dotted_line_length, right_ankle[1], (255, 0, 0))

        # 计算膝髋夹角
        knee_hip_angle = calculate_angle(k[12].cpu(), k[14].cpu(), reference_direction='vertical')

        # 绘制计数
        draw_text(
            im0,
            "CORRECT: " + str(self.state_tracker['SQUAT_COUNT']),
            pos=(int(frame_width * 0.68), 135),
            text_color=(255, 255, 115),
            font_scale=0.7,
            text_color_bg=(18, 185, 0)
        )
        # 调试：在屏幕上显示angle值
        draw_text(
            im0,
            "ANGLE: " + str(round(knee_hip_angle, 2)),
            pos=(int(frame_width * 0.68), 180),
            text_color=(255, 255, 255),
            font_scale=0.7,
            text_color_bg=(0, 0, 0)
        )
        # 获取当前状态
        current_state = self._get_state(knee_hip_angle)
        self.state_tracker['curr_state'] = current_state
        self._update_state_sequence(current_state)

        # 深蹲计数逻辑
        if current_state == 's1':
            if len(self.state_tracker['state_seq']) == 3 and not self.state_tracker['INCORRECT_POSTURE']:
                self.state_tracker['SQUAT_COUNT'] += 1
                count[0] += 1  # 更新传入的计数器列表中的第一个元素
            elif 's2' in self.state_tracker['state_seq'] and len(self.state_tracker['state_seq']) == 1:
                self.state_tracker['IMPROPER_SQUAT'] += 1
            elif self.state_tracker['INCORRECT_POSTURE']:
                self.state_tracker['IMPROPER_SQUAT'] += 1
            self.state_tracker['state_seq'] = []
            self.state_tracker['INCORRECT_POSTURE'] = False
        # 反馈显示逻辑
        else:
            hip_vertical_angle = calculate_angle(k[6].cpu(), k[12].cpu(), reference_direction="vertical")
            knee_vertical_angle = calculate_angle(k[12].cpu(), k[14].cpu(), reference_direction="vertical")
            ankle_vertical_angle = calculate_angle(k[14].cpu(), k[16].cpu(), reference_direction="vertical")

            if hip_vertical_angle > self.thresholds['HIP_THRESH'][1]:
                self.state_tracker['DISPLAY_TEXT'][0] = True
            elif hip_vertical_angle < self.thresholds['HIP_THRESH'][0] and self.state_tracker['state_seq'].count(
                    's2') == 1:
                self.state_tracker['DISPLAY_TEXT'][1] = True

            if self.thresholds['KNEE_THRESH'][0] < knee_vertical_angle < self.thresholds['KNEE_THRESH'][1] and \
                    self.state_tracker['state_seq'].count('s2') == 1:
                self.state_tracker['LOWER_HIPS'] = True
            elif knee_vertical_angle > self.thresholds['KNEE_THRESH'][2]:
                self.state_tracker['DISPLAY_TEXT'][3] = True
                self.state_tracker['INCORRECT_POSTURE'] = True

            if ankle_vertical_angle > self.thresholds['ANKLE_THRESH']:
                self.state_tracker['DISPLAY_TEXT'][2] = True
                self.state_tracker['INCORRECT_POSTURE'] = True

            self.state_tracker['COUNT_FRAMES'][self.state_tracker['DISPLAY_TEXT']] += 1
            im0 = _show_feedback(im0, self.state_tracker['COUNT_FRAMES'], self.FEEDBACK_ID_MAP)
        return im0