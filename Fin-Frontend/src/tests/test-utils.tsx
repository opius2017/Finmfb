import { ReactElement } from 'react';
import { Provider } from 'react-redux';
import { render } from '@testing-library/react';
import { store } from '../store/store';
import { MemoryRouter } from 'react-router-dom';

export function renderWithProvider(ui: ReactElement) {
  return render(
    <Provider store={store}>
      <MemoryRouter>
        {ui}
      </MemoryRouter>
    </Provider>
  );
}
