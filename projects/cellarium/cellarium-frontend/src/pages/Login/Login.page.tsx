import { useAuth } from "react-oidc-context";
import { Navigate } from "react-router-dom";

export const LoginPage = () => {
  const auth = useAuth();

  if (auth.isAuthenticated) {
    return <Navigate to={'/app'}/>
  }

  return <button onClick={() => void auth.signinRedirect()}>Log in</button>
};