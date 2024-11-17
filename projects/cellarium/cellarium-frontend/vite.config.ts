import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import path from "path";
import { pathToFileURL } from "url";
import {FileImporter} from "sass-embedded";
import sassDts from 'vite-plugin-sass-dts'


// https://vite.dev/config/
export default defineConfig( ({mode})=>{

  const env = loadEnv(mode, process.cwd(), '')
  return{
    plugins: [
      react(),
      sassDts({
        //global: false,
        // {
        //   generate: false,
        //   outputFilePath: path.resolve(__dirname, "./src/style.d.ts"),
        // },
      }),
    ],
    server:{
      port: Number.parseInt(env.PORT),
    },
    resolve:{
      alias: {
        "@": path.resolve(__dirname, "./src/"),
      }
    },
    define: {
      //"services__Arachne__https__0": env['services__Arachne__https__0'],
      "services__Arachne__https__0": JSON.stringify(env['services__Arachne__https__0']),
      
    },
    css: {
      modules:{
        localsConvention: 'camelCase',
      },
      preprocessorOptions: {
        scss: {
          api: 'modern-compiler',
          //additionalData: `@use "@/styles";`,
          importers: [
            {
              findFileUrl: async function (url) {
                if (!url.startsWith('@/')) return null;
                const newPath = path.resolve(__dirname, './src', url.substring(2));
                return pathToFileURL(newPath);
              },
            } as FileImporter<'async'>,
          ],
        }
      }
    }
  }
})
