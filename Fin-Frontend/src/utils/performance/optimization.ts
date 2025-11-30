// Performance Optimization Utilities

// Debounce function
export function debounce<T extends (...args: any[]) => any>(
  func: T,
  wait: number
): (...args: Parameters<T>) => void {
  let timeout: NodeJS.Timeout | null = null;

  return function executedFunction(...args: Parameters<T>) {
    const later = () => {
      timeout = null;
      func(...args);
    };

    if (timeout) {
      clearTimeout(timeout);
    }
    timeout = setTimeout(later, wait);
  };
}

// Throttle function
export function throttle<T extends (...args: any[]) => any>(
  func: T,
  limit: number
): (...args: Parameters<T>) => void {
  let inThrottle: boolean;

  return function executedFunction(...args: Parameters<T>) {
    if (!inThrottle) {
      func(...args);
      inThrottle = true;
      setTimeout(() => (inThrottle = false), limit);
    }
  };
}

// Memoization
export function memoize<T extends (...args: any[]) => any>(
  func: T
): T {
  const cache = new Map<string, ReturnType<T>>();

  return ((...args: Parameters<T>) => {
    const key = JSON.stringify(args);
    
    if (cache.has(key)) {
      return cache.get(key);
    }

    const result = func(...args);
    cache.set(key, result);
    return result;
  }) as T;
}

// Request batching
export class RequestBatcher<T, R> {
  private queue: Array<{ args: T; resolve: (value: R) => void; reject: (error: any) => void }> = [];
  private batchFunction: (items: T[]) => Promise<R[]>;
  private batchSize: number;
  private batchDelay: number;
  private timeout: NodeJS.Timeout | null = null;

  constructor(
    batchFunction: (items: T[]) => Promise<R[]>,
    batchSize: number = 10,
    batchDelay: number = 50
  ) {
    this.batchFunction = batchFunction;
    this.batchSize = batchSize;
    this.batchDelay = batchDelay;
  }

  add(args: T): Promise<R> {
    return new Promise((resolve, reject) => {
      this.queue.push({ args, resolve, reject });

      if (this.queue.length >= this.batchSize) {
        this.flush();
      } else if (!this.timeout) {
        this.timeout = setTimeout(() => this.flush(), this.batchDelay);
      }
    });
  }

  private async flush(): Promise<void> {
    if (this.timeout) {
      clearTimeout(this.timeout);
      this.timeout = null;
    }

    if (this.queue.length === 0) return;

    const batch = this.queue.splice(0, this.batchSize);
    const args = batch.map(item => item.args);

    try {
      const results = await this.batchFunction(args);
      batch.forEach((item, index) => {
        item.resolve(results[index]);
      });
    } catch (error) {
      batch.forEach(item => {
        item.reject(error);
      });
    }
  }
}

// Performance monitoring
export class PerformanceMonitor {
  private marks: Map<string, number> = new Map();
  private measures: Map<string, number> = new Map();

  mark(name: string): void {
    this.marks.set(name, performance.now());
  }

  measure(name: string, startMark: string, endMark?: string): number {
    const start = this.marks.get(startMark);
    if (!start) {
      throw new Error(`Start mark "${startMark}" not found`);
    }

    const end = endMark ? this.marks.get(endMark) : performance.now();
    if (endMark && !end) {
      throw new Error(`End mark "${endMark}" not found`);
    }

    const duration = (end as number) - start;
    this.measures.set(name, duration);
    return duration;
  }

  getMeasure(name: string): number | undefined {
    return this.measures.get(name);
  }

  clearMarks(): void {
    this.marks.clear();
  }

  clearMeasures(): void {
    this.measures.clear();
  }

  getReport(): PerformanceReport {
    const measures: Record<string, number> = {};
    this.measures.forEach((value, key) => {
      measures[key] = value;
    });

    return {
      measures,
      memory: this.getMemoryUsage(),
      timestamp: Date.now(),
    };
  }

  private getMemoryUsage(): MemoryInfo | null {
    if ('memory' in performance) {
      const memory = (performance as any).memory;
      return {
        usedJSHeapSize: memory.usedJSHeapSize,
        totalJSHeapSize: memory.totalJSHeapSize,
        jsHeapSizeLimit: memory.jsHeapSizeLimit,
      };
    }
    return null;
  }
}

interface PerformanceReport {
  measures: Record<string, number>;
  memory: MemoryInfo | null;
  timestamp: number;
}

interface MemoryInfo {
  usedJSHeapSize: number;
  totalJSHeapSize: number;
  jsHeapSizeLimit: number;
}

// Global performance monitor
export const performanceMonitor = new PerformanceMonitor();

// Image optimization
export function optimizeImageUrl(url: string, width?: number, quality?: number): string {
  const params = new URLSearchParams();
  if (width) params.append('w', width.toString());
  if (quality) params.append('q', quality.toString());
  
  const separator = url.includes('?') ? '&' : '?';
  return params.toString() ? `${url}${separator}${params}` : url;
}

// Compression utilities
export function compressData(data: any): string {
  // Simple compression using JSON stringify
  // In production, use a proper compression library
  return JSON.stringify(data);
}

export function decompressData(compressed: string): any {
  return JSON.parse(compressed);
}

// Resource preloading
export function preloadResource(url: string, type: 'script' | 'style' | 'image' | 'font'): void {
  const link = document.createElement('link');
  link.rel = 'preload';
  link.href = url;
  link.as = type;
  
  if (type === 'font') {
    link.crossOrigin = 'anonymous';
  }
  
  document.head.appendChild(link);
}

// Code splitting helper
export async function loadComponent<T>(
  importFunc: () => Promise<{ default: T }>
): Promise<T> {
  try {
    const module = await importFunc();
    return module.default;
  } catch (error) {
    console.error('Failed to load component:', error);
    throw error;
  }
}
