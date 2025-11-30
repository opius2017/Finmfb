// Lazy Loading Utilities
export class LazyLoader {
  private observer: IntersectionObserver | null = null;
  private loadedElements: Set<Element> = new Set();

  constructor() {
    if (typeof window !== 'undefined' && 'IntersectionObserver' in window) {
      this.observer = new IntersectionObserver(
        (entries) => {
          entries.forEach((entry) => {
            if (entry.isIntersecting && !this.loadedElements.has(entry.target)) {
              this.loadElement(entry.target);
              this.loadedElements.add(entry.target);
            }
          });
        },
        {
          rootMargin: '50px',
          threshold: 0.01,
        }
      );
    }
  }

  observe(element: Element): void {
    if (this.observer) {
      this.observer.observe(element);
    }
  }

  unobserve(element: Element): void {
    if (this.observer) {
      this.observer.unobserve(element);
      this.loadedElements.delete(element);
    }
  }

  private loadElement(element: Element): void {
    // Load images
    if (element instanceof HTMLImageElement) {
      const src = element.dataset.src;
      if (src) {
        element.src = src;
        element.removeAttribute('data-src');
      }
    }

    // Load background images
    const bgImage = element.getAttribute('data-bg');
    if (bgImage) {
      (element as HTMLElement).style.backgroundImage = `url(${bgImage})`;
      element.removeAttribute('data-bg');
    }

    // Trigger custom load event
    const loadEvent = new CustomEvent('lazyload', { detail: { element } });
    element.dispatchEvent(loadEvent);
  }

  disconnect(): void {
    if (this.observer) {
      this.observer.disconnect();
      this.loadedElements.clear();
    }
  }
}

// Global lazy loader instance
export const globalLazyLoader = new LazyLoader();

// React hook for lazy loading
export function useLazyLoad(ref: React.RefObject<HTMLElement>) {
  React.useEffect(() => {
    const element = ref.current;
    if (element) {
      globalLazyLoader.observe(element);
      return () => globalLazyLoader.unobserve(element);
    }
  }, [ref]);
}

// Pagination helper
export class PaginationManager<T> {
  private items: T[];
  private pageSize: number;
  private currentPage: number;

  constructor(items: T[], pageSize: number = 20) {
    this.items = items;
    this.pageSize = pageSize;
    this.currentPage = 1;
  }

  getPage(page: number): T[] {
    const start = (page - 1) * this.pageSize;
    const end = start + this.pageSize;
    return this.items.slice(start, end);
  }

  getCurrentPage(): T[] {
    return this.getPage(this.currentPage);
  }

  nextPage(): T[] {
    if (this.hasNextPage()) {
      this.currentPage++;
    }
    return this.getCurrentPage();
  }

  previousPage(): T[] {
    if (this.hasPreviousPage()) {
      this.currentPage--;
    }
    return this.getCurrentPage();
  }

  goToPage(page: number): T[] {
    if (page >= 1 && page <= this.getTotalPages()) {
      this.currentPage = page;
    }
    return this.getCurrentPage();
  }

  hasNextPage(): boolean {
    return this.currentPage < this.getTotalPages();
  }

  hasPreviousPage(): boolean {
    return this.currentPage > 1;
  }

  getTotalPages(): number {
    return Math.ceil(this.items.length / this.pageSize);
  }

  getTotalItems(): number {
    return this.items.length;
  }

  getCurrentPageNumber(): number {
    return this.currentPage;
  }

  getPageSize(): number {
    return this.pageSize;
  }

  setPageSize(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
  }
}

// Virtual scrolling helper
export class VirtualScroller<T> {
  private items: T[];
  private itemHeight: number;
  private containerHeight: number;
  private overscan: number;

  constructor(
    items: T[],
    itemHeight: number,
    containerHeight: number,
    overscan: number = 3
  ) {
    this.items = items;
    this.itemHeight = itemHeight;
    this.containerHeight = containerHeight;
    this.overscan = overscan;
  }

  getVisibleRange(scrollTop: number): { start: number; end: number; offsetY: number } {
    const start = Math.max(0, Math.floor(scrollTop / this.itemHeight) - this.overscan);
    const visibleCount = Math.ceil(this.containerHeight / this.itemHeight);
    const end = Math.min(
      this.items.length,
      start + visibleCount + this.overscan * 2
    );

    return {
      start,
      end,
      offsetY: start * this.itemHeight,
    };
  }

  getVisibleItems(scrollTop: number): T[] {
    const { start, end } = this.getVisibleRange(scrollTop);
    return this.items.slice(start, end);
  }

  getTotalHeight(): number {
    return this.items.length * this.itemHeight;
  }
}
