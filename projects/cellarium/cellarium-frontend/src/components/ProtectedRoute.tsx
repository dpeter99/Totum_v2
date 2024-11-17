import { Navigate } from "react-router-dom";
import {ReactChildren} from "@/utils";

type Props = {
    authenticated: boolean;
    redirectPath: string;
} & ReactChildren;

function ProtectedRoute({authenticated, children, redirectPath = '/'}: Props) {
    if (!authenticated) {
        return <Navigate to={redirectPath} replace />;
    }

    return children;
};

export default ProtectedRoute;