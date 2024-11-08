import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import path from "node:path";


// https://vite.dev/config/
export default defineConfig( ({mode})=>{

  const env = loadEnv(mode, process.cwd(), '')
  return{
    plugins: [react()],
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
      
    }
  }
})
