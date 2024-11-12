import { BrowserRouter, Routes, Route, RouteProps, Outlet, Navigate } from "react-router-dom";
import {AuthContextProps, AuthProvider, useAuth } from 'react-oidc-context';
import {arachneConfig} from "@/config/auth.ts";

import {asClass, asValue, AwilixContainer, createContainer, Lifetime } from "awilix/browser"
import { createContext, useCallback, useContext, useSyncExternalStore } from "react";
import {ReactChildren} from "@/utils";
import {LiveData} from "@/lib/live-data/LiveData.ts";
import {getResources} from "@/services/Api.ts";

type Weather = {
    temperatureC: number;
    summary: string;
}

export const Root = () => {
    return(
        <AuthProvider {...arachneConfig.settings}>
            <App />
        </AuthProvider>
    )
}

function App() {
    const auth = useAuth();

    switch (auth.activeNavigator) {
        case "signinSilent":
            return <div>Signing you in...</div>;
        case "signoutRedirect":
            return <div>Signing you out...</div>;
    }

    if (auth.isLoading) {
        return <div>Loading...</div>;
    }

    if (auth.error) {
        return <div>Oops... {auth.error.message}</div>;
    }
    
    return (
        <AppHostProvider>
            <BrowserRouter>
                <Routes>
                    <Route index element={<LoginPage/>} />
                    <Route path="/app" element={<Protected auth={auth}><Layout/></Protected>}>
                        <Route index element={<Home/>}/>
                        <Route path={'test'} element={<span>test</span>}/>
                    </Route>
                    <Route path={'/oauth/callback'} element={auth.isAuthenticated && auth.activeNavigator === undefined ? <Navigate to={'/'}/> : null}/>
                </Routes>
            </BrowserRouter>
        </AppHostProvider>
    );
}

type AppHostCradle = {
    UserService: UserService,
    AuthProvider: AuthContextProps,
    WeatherApiService: WeatherApiService
};

type AppHost = {
    container: AwilixContainer<AppHostCradle>;
}

let appHostInstance: AppHost | null = null;

const AppHostContext = createContext<AppHost>(null!)

const AppHostProvider = ({children}: ReactChildren) => {
    
    const auth = useAuth();
    
    if(!appHostInstance){
        const container = createContainer<AppHostCradle>({
            strict: true,
        });
        
        container.register('UserService', asClass(UserService).setLifetime(Lifetime.SINGLETON));
        container.register('WeatherApiService', asClass(WeatherApiService).setLifetime(Lifetime.SINGLETON));
        container.register('AuthProvider', asValue(auth));
        
        appHostInstance = {
            container,
        };
    }
    
    return (
        <AppHostContext.Provider value={appHostInstance}>
            {children}
        </AppHostContext.Provider>
    )
}

const useAppHost = () => {
    return useContext(AppHostContext);
}

const useService = <K extends keyof AppHostCradle>(name: K): AppHostCradle[K] => {
    const {container} = useAppHost();

    return container.resolve(name);
};


const useData = <T,>(data: LiveData<T>) =>{
    const subscribe = useCallback((s: () => void)=>data.subscribe(s), [data])
    
    return useSyncExternalStore<T>(subscribe, () => data.value);
}



class UserService {
    private authService: AuthContextProps;
    constructor(opts: AppHostCradle) {
        this.authService = opts.AuthProvider;
    }

    get userName() {
        return this.authService.user?.profile.name ?? '';
    }

    Logout() {
        this.authService.removeUser();
    }
}

class WeatherApiService {
    private authService: AuthContextProps; 
    constructor(opts: AppHostCradle) {
        this.authService = opts.AuthProvider;
    }
    
    public weather: LiveData<Weather[]> = new LiveData<Weather[]>([], ()=>this.getWeather());
    
    async getWeather(){
        if(!this.authService.isAuthenticated)
            return;
        
        const accessToken = this.authService.user!.access_token;
        const data = await getResources(accessToken);
        this.weather.setValue(data);
    }
    
}




type AuthRouteProps = {
    auth: AuthContextProps
} & RouteProps;

const Protected = (props: AuthRouteProps) => {
    return props.auth.isAuthenticated ? props.children : (<Navigate to="/" replace />);
}

const Layout = () => {
    const userService = useService('UserService');
        
    return (
      <div>
          <div>
              Header {userService.userName}
              <button onClick={() => void userService.Logout()}>Log out</button>
          </div>
          <main>
              <Outlet/>
          </main>
      </div>
    )
}

const LoginPage = () => {
    const auth = useAuth();
    
    if(auth.isAuthenticated){
        return <Navigate to={'/app'}/>
    }
    
    return <button onClick={() => void auth.signinRedirect()}>Log in</button>
};

function Home() {
    const weatherAPI = useService('WeatherApiService');
    const weather = useData(weatherAPI.weather);
    
    return (
        <div>
            Home page
            {weather.map((w,i)=>(
                <div key={i}>
                    {w.summary} : {w.temperatureC}
                </div>
            ))}
        </div>
    );
}

export default App;