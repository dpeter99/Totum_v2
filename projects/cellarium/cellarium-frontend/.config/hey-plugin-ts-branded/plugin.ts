import type { Plugin } from '@hey-api/openapi-ts';

import type { Config } from './types';
import {IRSchemaObject} from "@hey-api/openapi-ts/src/ir/types";

const parseSchema = (schema: IRSchemaObject) =>{
    
    if(schema.type === 'object'){
        if(!schema.properties) return; 
        for(let key in schema.properties){
            let value = schema.properties[key];
            parseSchema(value);
        }
    }
    else if(schema.type === 'array'){
        //For later to do
    }
    else if(schema.type === 'string'){
        if(typeof (schema.format) === 'string' &&
            schema.format.startsWith('brand::')) {
            let brand_name = schema.format.match(/brand::(.*)/)[1]
            // @ts-ignore
            schema.type = `Brand<${schema.type}, '${brand_name}'>`;
        }
    }
    
}

export const handler: Plugin.Handler<Config> = ({ context, plugin }) => {

    context.subscribe('schema', (props) => {
        // do something with the schema model
        parseSchema(props.schema);
        
    });
};