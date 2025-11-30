import { errorHandler, AppError, ErrorCode, createNotFoundError } from '../errorHandler';
import { mockRequest, mockResponse, mockNext } from '../../tests/setup';

describe('Error Handler Middleware', () => {
  let req: any;
  let res: any;
  let next: any;

  beforeEach(() => {
    req = mockRequest();
    res = mockResponse();
    next = mockNext();
  });

  it('should handle AppError correctly', () => {
    const error = new AppError(404, ErrorCode.NOT_FOUND, 'Resource not found');
    
    errorHandler(error, req, res, next);

    expect(res.status).toHaveBeenCalledWith(404);
    expect(res.json).toHaveBeenCalledWith({
      error: expect.objectContaining({
        code: ErrorCode.NOT_FOUND,
        message: 'Resource not found',
        correlationId: 'test-correlation-id',
      }),
    });
  });

  it('should handle generic Error as internal server error', () => {
    const error = new Error('Something went wrong');
    
    errorHandler(error, req, res, next);

    expect(res.status).toHaveBeenCalledWith(500);
    expect(res.json).toHaveBeenCalledWith({
      error: expect.objectContaining({
        code: ErrorCode.INTERNAL_ERROR,
        message: 'An unexpected error occurred',
      }),
    });
  });

  it('should include correlation ID in error response', () => {
    const error = new Error('Test error');
    req.correlationId = 'custom-correlation-id';
    
    errorHandler(error, req, res, next);

    expect(res.json).toHaveBeenCalledWith({
      error: expect.objectContaining({
        correlationId: 'custom-correlation-id',
      }),
    });
  });

  it('should create not found error correctly', () => {
    const error = createNotFoundError('User');
    
    expect(error).toBeInstanceOf(AppError);
    expect(error.statusCode).toBe(404);
    expect(error.code).toBe(ErrorCode.NOT_FOUND);
    expect(error.message).toBe('User not found');
  });
});
