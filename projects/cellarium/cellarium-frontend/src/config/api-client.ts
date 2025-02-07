import {client, CreateClientConfig} from "@/api/client.gen.ts";

export const createClientConfig: CreateClientConfig = () => {
    return {
        baseUrl: 'http://localhost:5002/'
    }
}

client.setConfig(createClientConfig());