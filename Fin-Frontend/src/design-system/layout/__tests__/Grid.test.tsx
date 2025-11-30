import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import { Grid, GridItem } from '../Grid';

describe('Grid Component', () => {
  describe('Basic Rendering', () => {
    it('renders grid container', () => {
      render(
        <Grid>
          <div>Item 1</div>
          <div>Item 2</div>
        </Grid>
      );
      
      expect(screen.getByText('Item 1')).toBeInTheDocument();
      expect(screen.getByText('Item 2')).toBeInTheDocument();
    });

    it('applies default 12 columns', () => {
      const { container } = render(
        <Grid>
          <div>Content</div>
        </Grid>
      );
      
      const grid = container.firstChild;
      expect(grid).toHaveClass('grid', 'grid-cols-12');
    });
  });

  describe('Column Configuration', () => {
    it('applies specified number of columns', () => {
      const { container } = render(
        <Grid cols={4}>
          <div>Content</div>
        </Grid>
      );
      
      const grid = container.firstChild;
      expect(grid).toHaveClass('grid-cols-4');
    });

    it('applies responsive column configuration', () => {
      const { container } = render(
        <Grid cols={1} responsive={{ sm: 2, md: 3, lg: 4 }}>
          <div>Content</div>
        </Grid>
      );
      
      const grid = container.firstChild;
      expect(grid).toHaveClass('grid-cols-1');
      expect(grid).toHaveClass('sm:grid-cols-2');
      expect(grid).toHaveClass('md:grid-cols-3');
      expect(grid).toHaveClass('lg:grid-cols-4');
    });
  });

  describe('Gap Configuration', () => {
    it('applies default medium gap', () => {
      const { container } = render(
        <Grid>
          <div>Content</div>
        </Grid>
      );
      
      const grid = container.firstChild;
      expect(grid).toHaveClass('gap-4');
    });

    it('applies specified gap size', () => {
      const { container } = render(
        <Grid gap="lg">
          <div>Content</div>
        </Grid>
      );
      
      const grid = container.firstChild;
      expect(grid).toHaveClass('gap-6');
    });
  });

  describe('Custom Styling', () => {
    it('applies custom className', () => {
      const { container } = render(
        <Grid className="custom-grid">
          <div>Content</div>
        </Grid>
      );
      
      const grid = container.firstChild;
      expect(grid).toHaveClass('custom-grid');
    });
  });
});

describe('GridItem Component', () => {
  describe('Basic Rendering', () => {
    it('renders grid item', () => {
      render(
        <GridItem>
          <div>Item Content</div>
        </GridItem>
      );
      
      expect(screen.getByText('Item Content')).toBeInTheDocument();
    });

    it('applies default span of 1', () => {
      const { container } = render(
        <GridItem>
          <div>Content</div>
        </GridItem>
      );
      
      const item = container.firstChild;
      expect(item).toHaveClass('col-span-1');
    });
  });

  describe('Span Configuration', () => {
    it('applies specified span', () => {
      const { container } = render(
        <GridItem span={6}>
          <div>Content</div>
        </GridItem>
      );
      
      const item = container.firstChild;
      expect(item).toHaveClass('col-span-6');
    });

    it('applies responsive span configuration', () => {
      const { container } = render(
        <GridItem span={12} responsive={{ sm: 6, md: 4, lg: 3 }}>
          <div>Content</div>
        </GridItem>
      );
      
      const item = container.firstChild;
      expect(item).toHaveClass('col-span-12');
      expect(item).toHaveClass('sm:col-span-6');
      expect(item).toHaveClass('md:col-span-4');
      expect(item).toHaveClass('lg:col-span-3');
    });
  });

  describe('Custom Styling', () => {
    it('applies custom className', () => {
      const { container } = render(
        <GridItem className="custom-item">
          <div>Content</div>
        </GridItem>
      );
      
      const item = container.firstChild;
      expect(item).toHaveClass('custom-item');
    });
  });
});

describe('Grid Integration', () => {
  it('renders grid with multiple items', () => {
    render(
      <Grid cols={3} gap="md">
        <GridItem span={1}>
          <div>Item 1</div>
        </GridItem>
        <GridItem span={2}>
          <div>Item 2</div>
        </GridItem>
        <GridItem span={3}>
          <div>Item 3</div>
        </GridItem>
      </Grid>
    );
    
    expect(screen.getByText('Item 1')).toBeInTheDocument();
    expect(screen.getByText('Item 2')).toBeInTheDocument();
    expect(screen.getByText('Item 3')).toBeInTheDocument();
  });

  it('creates responsive layout', () => {
    const { container } = render(
      <Grid cols={1} responsive={{ md: 2, lg: 3 }}>
        <GridItem span={1} responsive={{ md: 1, lg: 2 }}>
          <div>Responsive Item</div>
        </GridItem>
      </Grid>
    );
    
    const grid = container.firstChild;
    const item = grid?.firstChild;
    
    expect(grid).toHaveClass('grid-cols-1', 'md:grid-cols-2', 'lg:grid-cols-3');
    expect(item).toHaveClass('col-span-1', 'md:col-span-1', 'lg:col-span-2');
  });
});
