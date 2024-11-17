import {AwilixContainer, createContainer} from "awilix/browser";
import {createContext, FC} from "react";
import {ReactChildren} from "@/utils";

export type ServicesType = {
    [K in string]: any; 
}

export type AppHostServices<A> = A extends AppHost<infer S> ? S : never;

export const AppHostContext = createContext<AppHost<any>>(null!)

export class AppHost<S extends ServicesType>{
    private container: AwilixContainer<S>;
    

    constructor() {
        this.container = createContainer<S>({
            strict: true,
        });
    }
    
    public init(clb: (c: AwilixContainer<ServicesType>) => void){
        clb(this.container);
    }
    
    get Container () {return this.container}
    
    public get Provider () : FC<ReactChildren> {
        return ({children}) =>{
            return (
              <AppHostContext.Provider value={this}>
                  {children}
              </AppHostContext.Provider>
            )
        }
    }
    
    public useService<K extends keyof S>(name: K): S[K]{
        const container = this.Container;

        return container.resolve(name);
    }
}