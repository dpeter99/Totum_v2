import {defaultConfig, defineConfig} from "./hey-plugin-ts-branded";
import {defaultPlugins} from "@hey-api/openapi-ts/src";


export default {
    input: '../cellarium-backend/cellarium-backend.json',
    output: 'src/api',
    plugins: [
        defineConfig({
            name: 'hey-plugin-ts-branded',
            myOption: true,
        }),
        {
            name: '@hey-api/sdk',
            asClass: true,
            serviceNameBuilder: '{{name}}Api',
        },
        '@hey-api/client-fetch',
        '@hey-api/typescript',
    ],
};