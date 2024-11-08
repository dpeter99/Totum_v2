import { useEffect, useState } from 'react';
import {getUser, logout, User} from '@/services/AuthService'
import { BrowserRouter, Routes, Route } from "react-router-dom";
import UnAuthenticated from './pages/unauthenticated.page';
import ProtectedRoute from './components/ProtectedRoute';
import OAuthCallback from './pages/oauth-callback.page';
import {getResources} from "@/services/Api.ts";

type Weather = {
    temperatureC: number;
    summary: string;
}

function App() {
    const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
    const [user, setUser] = useState<User | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);

    const [data, setData] = useState<Weather[]>([]);
    
    async function fetchData() {
        const user = await getUser();
        const accessToken = user?.access_token;
        
        setUser(user);
        
        if (accessToken) {
            setIsAuthenticated(true);

            const data = await getResources(accessToken);
            setData(data);
        }

        setIsLoading(false);
    }

    useEffect(() => {
        fetchData();
    }, [isAuthenticated]);

    if (isLoading) {
        return (<>Loading...</>)
    }

    return (
        <BrowserRouter>
            <Routes>
                <Route path={'/'} element={<UnAuthenticated authenticated={isAuthenticated} />} />

                <Route path={'/resources'} element={
                    <ProtectedRoute authenticated={isAuthenticated} redirectPath='/'>
                        <span>Authenticated OAuth Server result:</span>
                        <br/>
                        {JSON.stringify(user)}
                        <br/>
                        <h1>{user?.profile.name}</h1>
                        { data.map((w, i)=> (
                            <li key={i}>{w.summary}: {w.temperatureC}</li>
                        )) }
                        <button onClick={logout}>Log out</button>
                    </ProtectedRoute>
                } />

                <Route path='/oauth/callback' element={<OAuthCallback setIsAuthenticated={setIsAuthenticated} />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;