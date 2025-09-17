import React, { ReactElement } from 'react';
import { Provider } from 'react-redux';
import { render } from '@testing-library/react';
import { store } from '../store/store';

export function renderWithProvider(ui: ReactElement) {
  return render(<Provider store={store}>{ui}</Provider>);
}
