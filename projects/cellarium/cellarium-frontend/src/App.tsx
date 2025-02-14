import {BrowserRouter, Navigate, Route, RouteProps, Routes} from "react-router-dom";
import {AuthContextProps, AuthProvider, useAuth} from 'react-oidc-context';
import {arachneConfig} from "@/config/auth.ts";

import {asClass, asValue, Lifetime} from "awilix/browser"
import {ShoppingListService} from "@/services/ShoppingListService.ts";
import {AppHost} from "@/lib/app-host";
import {Layout} from "@/pages/Layout";
import {LoginPage} from "@/pages/Login/Login.page.tsx";
import {Home} from "@/pages/Home/Home.page.tsx";
import {UserService} from "@/services/UserService.ts";
import {WeatherApiService} from "@/services/WeatherApiService.ts";
import {ShoppingListPage} from "@/pages/ShopingList/ShoppingList.page.tsx";
import {client} from "@/api/client.gen.ts";



export const Root = () => {
    return(
        <AuthProvider {...arachneConfig.settings}>
            <App />
        </AuthProvider>
    )
}

export type Services = {
    UserService: UserService,
    AuthProvider: AuthContextProps,
    ShoppingListService: ShoppingListService
};

export const host = new AppHost<Services>();

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
    
    client.setConfig({
        baseUrl: 'https://localhost:5002/',
        auth: auth.user?.access_token ?? ""    
    })

    host.init((container) => {
        container.register('UserService', asClass(UserService).setLifetime(Lifetime.SINGLETON));
        container.register('WeatherApiService', asClass(WeatherApiService).setLifetime(Lifetime.SINGLETON));
        container.register('AuthProvider', asValue(auth));

        container.register('ShoppingListService', asClass(ShoppingListService).setLifetime(Lifetime.TRANSIENT));
    })
    
    return (
      <host.Provider>
          <BrowserRouter
            future={{
                v7_startTransition: true,
                v7_relativeSplatPath: true,
            }}
          >
              <Routes>
                  <Route index element={<LoginPage/>}/>
                  <Route path="/app" element={<Protected auth={auth}><Layout/></Protected>}>
                      <Route index element={<Home/>}/>
                      <Route path={'test'} element={<span>test</span>}/>
                      <Route path={'shoppingList'} element={<ShoppingListPage></ShoppingListPage>}/>
                  </Route>
                  <Route path={'/oauth/callback'} element={auth.isAuthenticated && auth.activeNavigator === undefined ?
                    <Navigate to={'/'}/> : null}/>
              </Routes>
          </BrowserRouter>
      </host.Provider>
    );
}




type AuthRouteProps = {
    auth: AuthContextProps
} & RouteProps;

const Protected = (props: AuthRouteProps) => {
    return props.auth.isAuthenticated ? props.children : (<Navigate to="/" replace />);
}





export default App;