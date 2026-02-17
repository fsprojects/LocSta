import { defineConfig } from "vite";

export default defineConfig({
  root: "src/demoSite",
  base: "./",
  build: {
    outDir: "../../dist"
  }
});
