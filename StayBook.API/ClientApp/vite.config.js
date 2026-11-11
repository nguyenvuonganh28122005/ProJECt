import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { fileURLToPath } from 'node:url';

const projectRoot = fileURLToPath(new URL('..', import.meta.url));

export default defineConfig({
  plugins: [react()],
  base: '/',
  build: {
    outDir: fileURLToPath(new URL('../wwwroot', import.meta.url)),
    emptyOutDir: true
  },
  server: {
    host: 'localhost',
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': 'http://localhost:5131'
    }
  }
});
