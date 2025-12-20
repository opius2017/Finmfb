import axios from 'axios';

export const apiClient = axios.create({
  baseURL: 'http://localhost:5002/api', // Update with your backend URL
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token'); // Or however you store your token
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
