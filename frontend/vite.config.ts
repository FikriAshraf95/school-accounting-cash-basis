import { defineConfig, loadEnv } from "vite";
import vue from "@vitejs/plugin-vue";
import path from "node:path";
import tailwind from "tailwindcss";
import autoprefixer from "autoprefixer";

export default defineConfig(({ mode }) => {
  // Load environment variables based on the `mode` (development, production)
  const env = loadEnv(mode, process.cwd());

  return {
    // Base public path, configurable by environment
    base: env.VITE_BASE_PATH || "/",

    server: {
      host: "0.0.0.0",
      port: env.VITE_PORT ? parseInt(env.VITE_PORT) : 3000, // Optional port setting
    },

    css: {
      postcss: {
        plugins: [tailwind(), autoprefixer()],
      },
    },

    plugins: [
      vue(),
    ],

    resolve: {
      alias: {
        "@": path.resolve(__dirname, "./src"),
      },
    },

    build: {
      sourcemap: mode !== "production", // Enable source maps only in non-production
      minify: "esbuild", // Use esbuild for fast minification
      target: "esnext",  // Target modern browsers
    },
  };
});
