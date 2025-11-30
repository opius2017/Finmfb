import { describe, it, expect, beforeEach, vi } from 'vitest';
import { CacheManager, globalCache } from '../caching';
import { PaginationManager, VirtualScroller } from '../lazyLoading';
import { debounce, throttle, memoize, RequestBatcher, PerformanceMonitor } from '../optimization';

describe('Performance Optimization Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Caching', () => {
    it('should store and retrieve cached values', () => {
      const cache = new CacheManager(10, 1000);
      
      cache.set('key1', 'value1');
      expect(cache.get('key1')).toBe('value1');
    });

    it('should return null for non-existent keys', () => {
      const cache = new CacheManager();
      
      expect(cache.get('nonexistent')).toBeNull();
    });

    it('should expire cached values after TTL', async () => {
      const cache = new CacheManager(10, 100); // 100ms TTL
      
      cache.set('key1', 'value1');
      expect(cache.get('key1')).toBe('value1');
      
      await new Promise(resolve => setTimeout(resolve, 150));
      expect(cache.get('key1')).toBeNull();
    });

    it('should respect max cache size', () => {
      const cache = new CacheManager(3, 10000);
      
      cache.set('key1', 'value1');
      cache.set('key2', 'value2');
      cache.set('key3', 'value3');
      cache.set('key4', 'value4'); // Should evict key1
      
      expect(cache.get('key1')).toBeNull();
      expect(cache.get('key4')).toBe('value4');
    });

    it('should clear all cached values', () => {
      const cache = new CacheManager();
      
      cache.set('key1', 'value1');
      cache.set('key2', 'value2');
      cache.clear();
      
      expect(cache.get('key1')).toBeNull();
      expect(cache.get('key2')).toBeNull();
    });

    it('should invalidate cache by pattern', () => {
      const cache = new CacheManager();
      
      cache.set('user:1', 'data1');
      cache.set('user:2', 'data2');
      cache.set('product:1', 'data3');
      
      cache.invalidatePattern(/^user:/);
      
      expect(cache.get('user:1')).toBeNull();
      expect(cache.get('user:2')).toBeNull();
      expect(cache.get('product:1')).toBe('data3');
    });
  });

  describe('Pagination', () => {
    const items = Array.from({ length: 100 }, (_, i) => i + 1);

    it('should paginate items correctly', () => {
      const paginator = new PaginationManager(items, 10);
      
      const page1 = paginator.getPage(1);
      expect(page1).toHaveLength(10);
      expect(page1[0]).toBe(1);
      expect(page1[9]).toBe(10);
    });

    it('should navigate to next page', () => {
      const paginator = new PaginationManager(items, 10);
      
      const page2 = paginator.nextPage();
      expect(page2[0]).toBe(11);
      expect(paginator.getCurrentPageNumber()).toBe(2);
    });

    it('should navigate to previous page', () => {
      const paginator = new PaginationManager(items, 10);
      
      paginator.goToPage(3);
      const page2 = paginator.previousPage();
      
      expect(page2[0]).toBe(11);
      expect(paginator.getCurrentPageNumber()).toBe(2);
    });

    it('should calculate total pages correctly', () => {
      const paginator = new PaginationManager(items, 10);
      
      expect(paginator.getTotalPages()).toBe(10);
    });

    it('should handle page size changes', () => {
      const paginator = new PaginationManager(items, 10);
      
      paginator.setPageSize(20);
      expect(paginator.getTotalPages()).toBe(5);
      expect(paginator.getCurrentPage()).toHaveLength(20);
    });
  });

  describe('Virtual Scrolling', () => {
    const items = Array.from({ length: 1000 }, (_, i) => ({ id: i, name: `Item ${i}` }));

    it('should calculate visible range correctly', () => {
      const scroller = new VirtualScroller(items, 50, 500);
      
      const range = scroller.getVisibleRange(0);
      expect(range.start).toBe(0);
      expect(range.offsetY).toBe(0);
    });

    it('should get visible items', () => {
      const scroller = new VirtualScroller(items, 50, 500);
      
      const visible = scroller.getVisibleItems(0);
      expect(visible.length).toBeGreaterThan(0);
      expect(visible.length).toBeLessThan(items.length);
    });

    it('should calculate total height', () => {
      const scroller = new VirtualScroller(items, 50, 500);
      
      expect(scroller.getTotalHeight()).toBe(50000); // 1000 items * 50px
    });
  });

  describe('Debounce', () => {
    it('should debounce function calls', async () => {
      const fn = vi.fn();
      const debounced = debounce(fn, 100);
      
      debounced();
      debounced();
      debounced();
      
      expect(fn).not.toHaveBeenCalled();
      
      await new Promise(resolve => setTimeout(resolve, 150));
      expect(fn).toHaveBeenCalledTimes(1);
    });
  });

  describe('Throttle', () => {
    it('should throttle function calls', async () => {
      const fn = vi.fn();
      const throttled = throttle(fn, 100);
      
      throttled();
      throttled();
      throttled();
      
      expect(fn).toHaveBeenCalledTimes(1);
      
      await new Promise(resolve => setTimeout(resolve, 150));
      throttled();
      expect(fn).toHaveBeenCalledTimes(2);
    });
  });

  describe('Memoization', () => {
    it('should memoize function results', () => {
      const fn = vi.fn((a: number, b: number) => a + b);
      const memoized = memoize(fn);
      
      expect(memoized(1, 2)).toBe(3);
      expect(memoized(1, 2)).toBe(3);
      expect(fn).toHaveBeenCalledTimes(1);
    });

    it('should cache different argument combinations separately', () => {
      const fn = vi.fn((a: number, b: number) => a + b);
      const memoized = memoize(fn);
      
      expect(memoized(1, 2)).toBe(3);
      expect(memoized(2, 3)).toBe(5);
      expect(fn).toHaveBeenCalledTimes(2);
    });
  });

  describe('Request Batching', () => {
    it('should batch requests', async () => {
      const batchFn = vi.fn(async (items: number[]) => 
        items.map(x => x * 2)
      );
      
      const batcher = new RequestBatcher(batchFn, 3, 50);
      
      const promises = [
        batcher.add(1),
        batcher.add(2),
        batcher.add(3),
      ];
      
      const results = await Promise.all(promises);
      
      expect(results).toEqual([2, 4, 6]);
      expect(batchFn).toHaveBeenCalledTimes(1);
      expect(batchFn).toHaveBeenCalledWith([1, 2, 3]);
    });

    it('should flush batch after delay', async () => {
      const batchFn = vi.fn(async (items: number[]) => 
        items.map(x => x * 2)
      );
      
      const batcher = new RequestBatcher(batchFn, 10, 50);
      
      const result = await batcher.add(1);
      
      expect(result).toBe(2);
      expect(batchFn).toHaveBeenCalledTimes(1);
    });
  });

  describe('Performance Monitoring', () => {
    it('should mark and measure performance', () => {
      const monitor = new PerformanceMonitor();
      
      monitor.mark('start');
      // Simulate some work
      for (let i = 0; i < 1000; i++) {
        Math.sqrt(i);
      }
      monitor.mark('end');
      
      const duration = monitor.measure('operation', 'start', 'end');
      
      expect(duration).toBeGreaterThan(0);
    });

    it('should get performance report', () => {
      const monitor = new PerformanceMonitor();
      
      monitor.mark('start');
      monitor.mark('end');
      monitor.measure('test', 'start', 'end');
      
      const report = monitor.getReport();
      
      expect(report.measures).toHaveProperty('test');
      expect(report.timestamp).toBeGreaterThan(0);
    });

    it('should clear marks and measures', () => {
      const monitor = new PerformanceMonitor();
      
      monitor.mark('start');
      monitor.measure('test', 'start');
      
      monitor.clearMarks();
      monitor.clearMeasures();
      
      expect(monitor.getMeasure('test')).toBeUndefined();
    });
  });

  describe('Large Dataset Handling', () => {
    it('should handle large datasets efficiently', () => {
      const largeDataset = Array.from({ length: 10000 }, (_, i) => ({
        id: i,
        name: `Item ${i}`,
        value: Math.random() * 1000,
      }));

      const paginator = new PaginationManager(largeDataset, 100);
      
      const startTime = performance.now();
      const page = paginator.getPage(1);
      const endTime = performance.now();
      
      expect(page).toHaveLength(100);
      expect(endTime - startTime).toBeLessThan(10); // Should be very fast
    });

    it('should cache frequently accessed data', () => {
      const cache = new CacheManager(100);
      const data = { large: 'dataset', with: 'many', fields: true };
      
      cache.set('large-data', data);
      
      const startTime = performance.now();
      const cached = cache.get('large-data');
      const endTime = performance.now();
      
      expect(cached).toEqual(data);
      expect(endTime - startTime).toBeLessThan(1); // Cache retrieval should be instant
    });
  });

  describe('Concurrent User Load', () => {
    it('should handle multiple concurrent requests', async () => {
      const batchFn = async (items: number[]) => {
        await new Promise(resolve => setTimeout(resolve, 10));
        return items.map(x => x * 2);
      };
      
      const batcher = new RequestBatcher(batchFn, 10, 20);
      
      const promises = Array.from({ length: 50 }, (_, i) => batcher.add(i));
      const results = await Promise.all(promises);
      
      expect(results).toHaveLength(50);
      expect(results[0]).toBe(0);
      expect(results[49]).toBe(98);
    });
  });
});
