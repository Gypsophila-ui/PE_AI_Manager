import numpy as np
from function import draw_text, calculate_angle, _show_feedback, draw_dotted_line, find_angle

class PushupTracker:
    def __init__(self):
        """
        初始化俯卧撑状态追踪器。
        """
        # 初始化状态跟踪字典
        self.state_tracker = {
            'state_seq': [],
            'INACTIVE_TIME': 0.0,
            'INACTIVE_TIME_FRONT': 0.0,
            'DISPLAY_TEXT': np.full((4,), False),
            'COUNT_FRAMES': np.zeros((4,), dtype=np.int64),
            'LOWER_BODY': False,
            'INCORRECT_POSTURE': False,
            'prev_state': None,
            'curr_state': None,
            'PUSHUP_COUNT': 0,
            'IMPROPER_PUSHUP': 0
        }

        # 设置反馈信息映射
        self.FEEDBACK_ID_MAP = {
            0: ('HIP ANGLE TOO LOW', 215, (255, 80, 80)),  # 臀部角度过低
            1: ('KNEE TOO LOW', 170, (255, 80, 80)),  # 膝盖角度过小，接触地面
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
            'SHOULDER_ELBOW_WRIST': {
                'NORMAL': (155, 180),  # 初始阶段
                'TRANS': (125, 150),  # 过渡阶段
                'PASS': (55, 120)  # 完成阶段
            },
            'HIP_THRESH': 0,  # 臀部不能过低
            'KNEE_THRESH': 0,  # 膝盖不能着地
            'INACTIVE_THRESH': 10.0,  # 静止时间阈值 (秒)
            'CNT_FRAME_THRESH': 50  # 维持帧数阈值
        }
        return thresholds


    def _get_state(self, elbow_shoulder_angle):
        """
        根据肘-肩角度和身体高度判断动作状态。

        参数:
            elbow_shoulder_angle: 肘部与肩部之间的夹角（度）
            body_height: 身体的高度（单位：厘米）

        返回:
            str: 姿势状态（如 's1', 's2' 等），如果不在范围内则返回 None。
            's1'：下放阶段，表示俯卧撑下放，肘-肩角度在正常范围，且身体高度合适。
            's2'：过渡阶段，表示俯卧撑动作处于上升的过渡阶段，肘-肩角度增加。
            's3'：完成阶段，表示动作完成，肘-肩角度达到最大值，且身体与地面平行。
        """
        angle = None

        # 根据肘部与肩部之间的角度判断阶段
        if self.thresholds['SHOULDER_ELBOW_WRIST']['NORMAL'][0] <= elbow_shoulder_angle <= \
                self.thresholds['SHOULDER_ELBOW_WRIST']['NORMAL'][1]:
            angle = 1  # 初始阶段
        elif self.thresholds['SHOULDER_ELBOW_WRIST']['TRANS'][0] <= elbow_shoulder_angle <= \
                self.thresholds['SHOULDER_ELBOW_WRIST']['TRANS'][1]:
            angle = 2  # 下降阶段
        elif self.thresholds['SHOULDER_ELBOW_WRIST']['PASS'][0] <= elbow_shoulder_angle <= \
                self.thresholds['SHOULDER_ELBOW_WRIST']['PASS'][1]:
            angle = 3  # 完成阶段

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
        处理俯卧撑检测与计数的主逻辑，包含状态跟踪、姿势检查和反馈显示。

        参数:
        - k: 姿势关键点
        - im0: 当前帧图像
        - ind: 当前处理对象的索引（对于单人模式，始终为0）
        - count: 计数器列表（对于单人模式，只包含一个元素）
        """
        frame_height, frame_width, _ = im0.shape

        # 计算肩肘腕角度
        shoulder_elbow_angle = calculate_angle(k[5].cpu(), k[7].cpu(), reference_direction='horizontal')
        elbow_wrist_angle = calculate_angle(k[7].cpu(), k[9].cpu(), reference_direction='horizontal')
        shoulder_elbow_wrist_angle = shoulder_elbow_angle + elbow_wrist_angle

        # 绘制正确和错误的pushup计数
        draw_text(
            im0,
            "CORRECT: " + str(self.state_tracker['PUSHUP_COUNT']),
            pos=(int(frame_width * 0.68), 130),
            text_color=(255, 255, 230),
            font_scale=0.7,
            text_color_bg=(18, 185, 0)
        )
        # 调试：在屏幕上显示angle值
        draw_text(
            im0,
            "ANGLE: " + str(round(shoulder_elbow_wrist_angle, 2)),
            pos=(int(frame_width * 0.68), 180),
            text_color=(255, 255, 255),
            font_scale=0.7,
            text_color_bg=(0, 0, 0)
        )
        # # 获取肩膝腕的坐标
        # right_shoulder = (int(k[5][0].cpu().item()), int(k[5][1].cpu().item()))
        # right_elbow = (int(k[7][0].cpu().item()), int(k[7][1].cpu().item()))
        # right_wrist = (int(k[9][0].cpu().item()), int(k[9][1].cpu().item()))

        # # 绘制肩肘腕虚线
        # dotted_line_length = 60
        # im0 = draw_dotted_line(im0, right_shoulder, right_shoulder[1] - dotted_line_length, right_shoulder[1],
        #                        (255, 0, 0))
        # im0 = draw_dotted_line(im0, right_elbow, right_elbow[1] - dotted_line_length, right_elbow[1], (255, 0, 0))
        # im0 = draw_dotted_line(im0, right_wrist, right_wrist[1] - dotted_line_length, right_wrist[1], (255, 0, 0))

        # 获取当前俯卧撑状态
        current_state = self._get_state(shoulder_elbow_wrist_angle)
        self.state_tracker['curr_state'] = current_state
        self._update_state_sequence(current_state)

        # 俯卧撑计数逻辑
        if current_state == 's1':
            if len(self.state_tracker['state_seq']) == 3 and not self.state_tracker['INCORRECT_POSTURE']:
                self.state_tracker['PUSHUP_COUNT'] += 1
                count[0] += 1  # 更新传入的计数器列表中的第一个元素
            elif 's2' in self.state_tracker['state_seq'] and len(self.state_tracker['state_seq']) == 1:
                self.state_tracker['IMPROPER_PUSHUP'] += 1
            elif self.state_tracker['INCORRECT_POSTURE']:
                self.state_tracker['IMPROPER_PUSHUP'] += 1
            self.state_tracker['state_seq'] = []
            self.state_tracker['INCORRECT_POSTURE'] = False

        # 状态检查与反馈
        else:
            hip_keen_vertical_angle = find_angle(k[14].cpu(), k[12].cpu(), ref_pt=np.array([1, 0]))
            knee_ankle_vertical_angle = find_angle(k[16].cpu(), k[14].cpu(), ref_pt=np.array([1, 0]))

            if hip_keen_vertical_angle < self.thresholds['HIP_THRESH']:
                self.state_tracker['DISPLAY_TEXT'][0] = True
                self.state_tracker['INCORRECT_POSTURE'] = True
            elif knee_ankle_vertical_angle < self.thresholds['KNEE_THRESH']:
                self.state_tracker['DISPLAY_TEXT'][1] = True
                self.state_tracker['INCORRECT_POSTURE'] = True

            self.state_tracker['COUNT_FRAMES'][self.state_tracker['DISPLAY_TEXT']] += 1
            im0 = _show_feedback(im0, self.state_tracker['COUNT_FRAMES'], self.FEEDBACK_ID_MAP)

        return im0