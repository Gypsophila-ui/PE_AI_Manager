import cv2
import numpy as np
from function import draw_text, calculate_angle, _show_feedback, draw_dotted_line
import time

class PushupTracker:
    def __init__(self):
        """
        初始化俯卧撑状态追踪器。
        """
        # 初始化状态跟踪字典
        self.state_tracker = {
            'state_seq': [],
            'DISPLAY_TEXT': np.full((3,), False),  # 修改为3个提示
            'COUNT_FRAMES': np.zeros((3,), dtype=np.int64),  # 修改为3个提示
            'INCORRECT_POSTURE': False,
            'curr_state': None,
            'PUSHUP_COUNT': 0,
            'IMPROPER_PUSHUP': 0,
            'BACK_ARCHED': False,  # 背部拱起标志
            'INSUFFICIENT_RANGE': False,  # 动作幅度不够标志
            'BODY_SINKING': False  # 身体下沉标志
        }

        # 添加事件跟踪
        self.events = []  # 存储关键事件
        self.current_pushup_id = None
        self.pushup_start_frame = None
        self.last_improper_count = 0

        # 设置反馈信息映射 - 修改为三个提示
        self.FEEDBACK_ID_MAP = {
            0: ('BACK ARCHED', 215, (255, 80, 80)),       # 背部拱起
            1: ('INSUFFICIENT RANGE', 170, (255, 80, 80)), # 动作幅度不够
            2: ('BODY SINKING', 125, (255, 80, 80))       # 身体下沉
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
                'NORMAL': (155, 180),  # 初始阶段（上升完成）
                'TRANS': (110, 150),   # 过渡阶段（下降中）
                'PASS': (55, 100)      # 完成阶段（下放到最低点）
            },
            'BACK_ARCH_ANGLE': 90,      # 背部拱起角度阈值（度）
            'MIN_ELBOW_ANGLE': 100,      # 肘部最小角度阈值（度）- 动作幅度不够
            'HIP_SHOULDER_DISTANCE_RATIO': 0.85,  # 髋部与肩部距离比例阈值（身体下沉）
            'INACTIVE_THRESH': 10.0,    # 静止时间阈值 (秒)
            'CNT_FRAME_THRESH': 50      # 维持帧数阈值
        }
        return thresholds

    def _get_state(self, elbow_shoulder_angle):
        """
        根据肘-肩角度判断动作状态。

        参数:
            elbow_shoulder_angle: 肘部与肩部之间的夹角（度）

        返回:
            str: 姿势状态（如 's1', 's2' 等），如果不在范围内则返回 None。
        """
        angle = None

        # 根据肘部与肩部之间的角度判断阶段
        if self.thresholds['SHOULDER_ELBOW_WRIST']['NORMAL'][0] <= elbow_shoulder_angle <= \
                self.thresholds['SHOULDER_ELBOW_WRIST']['NORMAL'][1]:
            angle = 1  # 初始阶段（上升完成）
        elif self.thresholds['SHOULDER_ELBOW_WRIST']['TRANS'][0] <= elbow_shoulder_angle <= \
                self.thresholds['SHOULDER_ELBOW_WRIST']['TRANS'][1]:
            angle = 2  # 过渡阶段（下降中）
        elif self.thresholds['SHOULDER_ELBOW_WRIST']['PASS'][0] <= elbow_shoulder_angle <= \
                self.thresholds['SHOULDER_ELBOW_WRIST']['PASS'][1]:
            angle = 3  # 完成阶段（下放到最低点）

        return f's{angle}' if angle else None

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

    def _get_current_error_reasons(self):
        """获取当前错误原因，包含更多详细信息"""
        errors = []
        for i, is_displaying in enumerate(self.state_tracker['DISPLAY_TEXT']):
            if is_displaying:
                error_id, error_msg, _ = self.FEEDBACK_ID_MAP.get(i, (f"ERROR_{i}", "", (0,0,0)))
                errors.append({
                    'code': error_id,
                    'message': error_msg,
                    'frame_time': time.time(),
                    'error_type': '姿势错误，下降幅度不足，肘部角度过大'
                })
        return errors

    def _record_events(self, frame_index, fps):
        """记录关键事件，正确绑定错误信息到每个俯卧撑"""
        # 确保events列表中的每个事件都有correct_count和incorrect_count键
        if self.events:
            for event in self.events:
                if 'correct_count' not in event:
                    event['correct_count'] = self.state_tracker['PUSHUP_COUNT']
                if 'incorrect_count' not in event:
                    event['incorrect_count'] = self.state_tracker['IMPROPER_PUSHUP']

        # 检测俯卧撑开始
        if self.state_tracker['curr_state'] == 's2' and self.current_pushup_id is None:
            self.current_pushup_id = f"pushup_{len([e for e in self.events if e['type'] == 'PUSHUP_START']) + 1}"
            self.pushup_start_frame = frame_index
            self.events.append({
                'event_id': self.current_pushup_id,
                'type': 'PUSHUP_START',
                'frame': frame_index,
                'timestamp': frame_index / fps if fps > 0 else 0,
                'correct_count': self.state_tracker['PUSHUP_COUNT'],
                'incorrect_count': self.state_tracker['IMPROPER_PUSHUP'],
                'pushup_number': len([e for e in self.events if e['type'] == 'PUSHUP_START']),
                'current_errors': []
            })

        # 检查并记录当前错误
        current_errors = self._get_current_error_reasons()
        if self.current_pushup_id and current_errors:
            for event in reversed(self.events):
                if event.get('event_id') == self.current_pushup_id and event['type'] == 'PUSHUP_START':
                    for error in current_errors:
                        if error not in event.get('current_errors', []):
                            if 'current_errors' not in event:
                                event['current_errors'] = []
                            event['current_errors'].append(error)
                    break

        # 检测俯卧撑完成（正确）
        if self.current_pushup_id and self.state_tracker['curr_state'] == 's1':
            is_correct_completion = (
                len(self.state_tracker['state_seq']) == 3 and
                not self.state_tracker['INCORRECT_POSTURE']
            )

            if is_correct_completion:
                start_event = None
                for event in reversed(self.events):
                    if event.get('event_id') == self.current_pushup_id and event['type'] == 'PUSHUP_START':
                        start_event = event
                        break

                self.events.append({
                    'event_id': self.current_pushup_id,
                    'type': 'PUSHUP_COMPLETE_CORRECT',
                    'frame': frame_index,
                    'timestamp': frame_index / fps if fps > 0 else 0,
                    'duration_frames': frame_index - self.pushup_start_frame if self.pushup_start_frame else 0,
                    'correct_count': self.state_tracker['PUSHUP_COUNT'],
                    'incorrect_count': self.state_tracker['IMPROPER_PUSHUP'],
                    'pushup_number': start_event.get('pushup_number', 0) if start_event else 0,
                    'errors_during_pushup': start_event.get('current_errors', []) if start_event else []
                })
                self.current_pushup_id = None
                self.pushup_start_frame = None

            elif self.state_tracker['INCORRECT_POSTURE'] or (
                's2' in self.state_tracker['state_seq'] and
                len(self.state_tracker['state_seq']) == 1
            ):
                start_event = None
                for event in reversed(self.events):
                    if event.get('event_id') == self.current_pushup_id and event['type'] == 'PUSHUP_START':
                        start_event = event
                        break

                self.events.append({
                    'event_id': self.current_pushup_id,
                    'type': 'PUSHUP_COMPLETE_INCORRECT',
                    'frame': frame_index,
                    'timestamp': frame_index / fps if fps > 0 else 0,
                    'error_reasons': self._get_current_error_reasons(),
                    'duration_frames': frame_index - self.pushup_start_frame if self.pushup_start_frame else 0,
                    'correct_count': self.state_tracker['PUSHUP_COUNT'],
                    'incorrect_count': self.state_tracker['IMPROPER_PUSHUP'],
                    'pushup_number': start_event.get('pushup_number', 0) if start_event else 0,
                    'errors_during_pushup': start_event.get('current_errors', []) if start_event else []
                })
                self.last_improper_count = self.state_tracker['IMPROPER_PUSHUP']
                self.current_pushup_id = None
                self.pushup_start_frame = None

    def track(self, k, im0, ind, count, fps=30):
        """
        处理俯卧撑检测与计数的主逻辑，包含状态跟踪、姿势检查和反馈显示。
        """
        frame_height, frame_width, _ = im0.shape

        # 定义关节点坐标
        right_shoulder = (int(k[6][0].cpu().item()), int(k[6][1].cpu().item()))
        right_elbow = (int(k[8][0].cpu().item()), int(k[8][1].cpu().item()))
        right_wrist = (int(k[10][0].cpu().item()), int(k[10][1].cpu().item()))
        right_hip = (int(k[12][0].cpu().item()), int(k[12][1].cpu().item()))

        # 绘制辅助线和角度指示器
        dotted_line_length = 60
        im0 = draw_dotted_line(im0, right_shoulder, right_shoulder[1] - dotted_line_length, right_shoulder[1], (255, 0, 0))
        im0 = draw_dotted_line(im0, right_elbow, right_elbow[1] - dotted_line_length, right_elbow[1], (255, 0, 0))
        im0 = draw_dotted_line(im0, right_wrist, right_wrist[1] - dotted_line_length, right_wrist[1], (255, 0, 0))

        # 计算关键角度和距离
        # 计算肘部角度（肩-肘-腕）
        elbow_angle = calculate_angle(right_shoulder, right_elbow, right_wrist)
        # 计算肘部角度（肘-腕）
        elbow_wrist_angle = calculate_angle(right_elbow, right_wrist)

        # 背部角度（计算背部拱起程度）
        # 使用肩部和髋部连线与垂直线的角度
        back_angle = calculate_angle(right_shoulder, right_hip, reference_direction="vertical")
        
        # 计算肩部到手腕的垂直距离（用于判断身体下沉）
        shoulder_wrist_distance = abs(right_shoulder[1] - right_wrist[1])
        hip_shoulder_distance = abs(right_hip[1] - right_shoulder[1])
        
        # 计算身体下沉比例（手腕到地面的距离与肩部到髋部距离的比例）
        if hip_shoulder_distance > 0:
            body_sink_ratio = shoulder_wrist_distance / hip_shoulder_distance
        else:
            body_sink_ratio = 1.0

        # 绘制骨架连线
        color_light_blue = (204, 204, 0)
        cv2.line(im0, right_shoulder, right_elbow, color_light_blue, 4)
        cv2.line(im0, right_elbow, right_wrist, color_light_blue, 4)

        # 绘制关节点
        color_yellow = (0, 255, 255)
        cv2.circle(im0, right_shoulder, 7, color_yellow, -1)
        cv2.circle(im0, right_elbow, 7, color_yellow, -1)
        cv2.circle(im0, right_wrist, 7, color_yellow, -1)

        # 绘制右侧肘部角度指示器
        cv2.ellipse(im0, tuple(right_elbow), (25, 25),
                    angle=0, startAngle=elbow_wrist_angle, endAngle=elbow_wrist_angle - elbow_angle,
                    color=(255, 255, 255), thickness=3)

        # 绘制计数
        draw_text(
            im0,
            "CORRECT: " + str(self.state_tracker['PUSHUP_COUNT']),
            pos=(int(frame_width * 0.68), 130),
            text_color=(255, 255, 230),
            font_scale=0.7,
            text_color_bg=(18, 185, 0)
        )

        draw_text(
            im0,
            "INCORRECT: " + str(self.state_tracker['IMPROPER_PUSHUP']),
            pos=(int(frame_width * 0.68), 175),
            text_color=(255, 255, 230),
            font_scale=0.7,
            text_color_bg=(221, 0, 0),
        )

        # 调试：在屏幕上显示angle值
        draw_text(
            im0,
            f"ELBOW: {round(elbow_angle, 1)}",
            pos=(int(frame_width * 0.68), 220),
            text_color=(255, 255, 255),
            font_scale=0.7,
            text_color_bg=(0, 0, 0)
        )
        
        draw_text(
            im0,
            f"BACK: {round(back_angle, 1)}",
            pos=(int(frame_width * 0.68), 250),
            text_color=(255, 255, 255),
            font_scale=0.7,
            text_color_bg=(0, 0, 0)
        )

        # 获取当前俯卧撑状态
        current_state = self._get_state(elbow_angle)
        self.state_tracker['curr_state'] = current_state
        self._update_state_sequence(current_state)

        # 俯卧撑计数逻辑
        if current_state == 's1':
            if len(self.state_tracker['state_seq']) == 3 and not self.state_tracker['INCORRECT_POSTURE']:
                self.state_tracker['PUSHUP_COUNT'] += 1
                count[0] += 1  # 更新传入的计数器列表中的第一个元素
            elif 's2' in self.state_tracker['state_seq'] and len(self.state_tracker['state_seq']) == 1:
                # 没有完成足够的下蹲就上升，动作幅度不够
                self.state_tracker['IMPROPER_PUSHUP'] += 1
                # 设置动作幅度不够的错误标志，用于显示反馈
                self.state_tracker['DISPLAY_TEXT'][1] = True
                self.state_tracker['INCORRECT_POSTURE'] = True
            elif self.state_tracker['INCORRECT_POSTURE']:
                self.state_tracker['IMPROPER_PUSHUP'] += 1
            self.state_tracker['state_seq'] = []
            self.state_tracker['INCORRECT_POSTURE'] = False
        # 反馈显示逻辑（当前不是s1状态时）
        else:
            # 重置所有错误标志
            self.state_tracker['DISPLAY_TEXT'][:] = False
            
            # 检查各种错误姿势
            # 1. 背部拱起检查
            if abs(back_angle) > self.thresholds['BACK_ARCH_ANGLE']:
                self.state_tracker['DISPLAY_TEXT'][0] = True
                self.state_tracker['INCORRECT_POSTURE'] = True

            # 3. 身体下沉检查
            elif body_sink_ratio < self.thresholds['HIP_SHOULDER_DISTANCE_RATIO']:
                self.state_tracker['DISPLAY_TEXT'][2] = True
                self.state_tracker['INCORRECT_POSTURE'] = True

            # 更新计数帧数
            self.state_tracker['COUNT_FRAMES'][self.state_tracker['DISPLAY_TEXT']] += 1
            
            # 显示反馈提示
            im0 = _show_feedback(im0, self.state_tracker['COUNT_FRAMES'], self.FEEDBACK_ID_MAP)

        # 重置显示文本
        self.state_tracker['DISPLAY_TEXT'][self.state_tracker['COUNT_FRAMES'] > self.thresholds['CNT_FRAME_THRESH']] = False
        self.state_tracker['COUNT_FRAMES'][self.state_tracker['COUNT_FRAMES'] > self.thresholds['CNT_FRAME_THRESH']] = 0    
        
        # 记录事件
        self._record_events(count[0], fps)

        # 准备返回数据
        result_data = {
            'processed_frame': im0,
            'correct_count': self.state_tracker['PUSHUP_COUNT'],
            'incorrect_count': self.state_tracker['IMPROPER_PUSHUP'],
            'total_count': self.state_tracker['PUSHUP_COUNT'] + self.state_tracker['IMPROPER_PUSHUP'],
            'events': self.events[-10:] if self.events else [],  # 只返回最近10个事件
        }

        return result_data
