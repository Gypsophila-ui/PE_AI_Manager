import apiClient from './axios';

// API错误处理函数
const handleApiError = (error, operationType) => {
  if (error.response) {
    // 服务器返回错误状态码
    const errorCode = error.response.data?.error_code || -99;

    // 只处理特定的用户友好错误
    const specificErrors = {
      '-21': {
        login: '用户不存在',
        register: '该用户已经注册过账号',
        password_change: '账号不存在'
      },
      '-22': {
        login: '账号无效',
        register: '该ID不在基准库内，无效',
        password_change: '账号信息异常'
      },
      '-23': {
        login: '登录密码输入错误',
        register: '密码格式错误',
        password_change: '输入的旧密码不正确'
      }
    };

    // 检查是否有特定错误信息
    if (specificErrors[errorCode] && specificErrors[errorCode][operationType]) {
      return {
        success: false,
        error_code: errorCode,
        message: specificErrors[errorCode][operationType]
      };
    }

    // 其他错误返回通用提示
    let genericMessage = '操作失败';
    if (operationType === 'login') {
      genericMessage = '登录失败';
    } else if (operationType === 'register') {
      genericMessage = '注册失败';
    } else if (operationType === 'password_change') {
      genericMessage = '密码修改失败';
    }
    return { success: false, error_code: errorCode, message: genericMessage };
  } else if (error.request) {
    // 请求已发送但没有收到响应
    let genericMessage = '操作失败';
    if (operationType === 'login') {
      genericMessage = '登录失败';
    } else if (operationType === 'register') {
      genericMessage = '注册失败';
    } else if (operationType === 'password_change') {
      genericMessage = '密码修改失败';
    }
    return { success: false, error_code: -99, message: genericMessage };
  } else {
    // 请求配置出错
    let genericMessage = '操作失败';
    if (operationType === 'login') {
      genericMessage = '登录失败';
    } else if (operationType === 'register') {
      genericMessage = '注册失败';
    } else if (operationType === 'password_change') {
      genericMessage = '密码修改失败';
    }
    return { success: false, error_code: -99, message: genericMessage };
  }
};

// SHA-256加密函数
const sha256 = async (str) => {
  const encoder = new TextEncoder();
  const data = encoder.encode(str);
  const hash = await crypto.subtle.digest('SHA-256', data);
  const hashArray = Array.from(new Uint8Array(hash));
  const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
  return hashHex;
};

// 教师登录
export const loginTeacher = async (teacher_id, password) => {
  try {
    // 将密码转换为SHA-256
    const passwordHash = await sha256(password);

    const response = await apiClient.post('/login_teacher', {
      teacher_id,
      password: passwordHash
    });

    return { success: true, data: response.data };
  } catch (error) {
    return handleApiError(error, 'login');
  }
};

// 学生登录
export const loginStudent = async (student_id, password) => {
  try {
    // 将密码转换为SHA-256
    const passwordHash = await sha256(password);

    const response = await apiClient.post('/login_student', {
      student_id,
      password: passwordHash
    });

    return { success: true, data: response.data };
  } catch (error) {
    return handleApiError(error, 'login');
  }
};

// 教师注册
export const registerTeacher = async (id, password, name, gender, title, college, department) => {
  try {
    const response = await apiClient.post('/new_teacher', {
      id,
      password,
      name,
      gender,
      title,
      college,
      department
    });

    return { success: true, data: response.data };
  } catch (error) {
    return handleApiError(error, 'register');
  }
};

// 学生注册
export const registerStudent = async (id, password, name, gender, major, college, department) => {
  try {
    const response = await apiClient.post('/new_student', {
      id,
      password,
      name,
      gender,
      major,
      college,
      department
    });

    return { success: true, data: response.data };
  } catch (error) {
    return handleApiError(error, 'register');
  }
};

// 教师修改密码
export const changeTeacherPassword = async (id, oldPassword, newPassword) => {
  try {
    // 将密码转换为SHA-256
    const oldPasswordHash = await sha256(oldPassword);
    const newPasswordHash = await sha256(newPassword);

    const response = await apiClient.post('/change_teacher_password', {
      id,
      old_password: oldPasswordHash,
      new_password: newPasswordHash
    });

    return { success: true, data: response.data };
  } catch (error) {
    return handleApiError(error, 'password_change');
  }
};

// 学生修改密码
export const changeStudentPassword = async (id, oldPassword, newPassword) => {
  try {
    // 将密码转换为SHA-256
    const oldPasswordHash = await sha256(oldPassword);
    const newPasswordHash = await sha256(newPassword);

    const response = await apiClient.post('/change_student_password', {
      id,
      old_password: oldPasswordHash,
      new_password: newPasswordHash
    });

    return { success: true, data: response.data };
  } catch (error) {
    return handleApiError(error, 'password_change');
  }
};

export default {
  loginTeacher,
  loginStudent,
  registerTeacher,
  registerStudent,
  changeTeacherPassword,
  changeStudentPassword
};
