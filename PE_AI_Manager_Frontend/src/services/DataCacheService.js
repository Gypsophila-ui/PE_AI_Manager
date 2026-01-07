// src/services/DataCacheService.js

class DataCacheService {
  constructor() {
    // 存储结构: Map<key, { data, timestamp }>
    this.cache = new Map();
    // 默认缓存 10 分钟
    this.defaultTTL = 10 * 60 * 1000;
  }

  /**
   * 核心方法：带缓存的请求包装器
   * @param {string} key 缓存的唯一标识
   * @param {function} fetchFn 如果没有缓存，要执行的异步请求函数
   * @param {number} ttl 有效期（毫秒）
   */
  async fetchWithCache(key, fetchFn, ttl = this.defaultTTL) {
    const cached = this.cache.get(key);

    // 检查缓存是否存在且未过期
    if (cached && (Date.now() - cached.timestamp < ttl)) {
      console.log(`%c[Cache Hit] ${key}`, 'color: #4CAF50; font-weight: bold');
      return cached.data;
    }

    console.log(`%c[Cache Miss] ${key}`, 'color: #FF9800; font-weight: bold');

    // 执行真正的请求
    try {
      const data = await fetchFn();
      // 存入缓存
      this.cache.set(key, {
        data,
        timestamp: Date.now()
      });
      return data;
    } catch (error) {
      console.error(`[Cache Fetch Error] ${key}:`, error);
      throw error;
    }
  }

  /**
   * 清除包含特定前缀的缓存
   * 例如：invalidate('ASSIGNMENTS') 会清除所有作业相关的缓存
   */
  invalidate(prefix) {
    for (const key of this.cache.keys()) {
      if (key.startsWith(prefix)) {
        this.cache.delete(key);
      }
    }
  }

  /**
   * 手动更新某个特定缓存（例如在编辑后）
   */
  update(key, newData) {
    if (this.cache.has(key)) {
      this.cache.set(key, {
        data: newData,
        timestamp: Date.now()
      });
    }
  }

  clearAll() {
    this.cache.clear();
  }
}

export const cacheService = new DataCacheService();
