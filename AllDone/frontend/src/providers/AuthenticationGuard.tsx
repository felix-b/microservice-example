import { withAuthenticationRequired } from "@auth0/auth0-react";
import { PageLoader } from "../components/PageLoader";

type AuthenticationGuardProps = {
    component: React.ComponentType
}

export const AuthenticationGuard: React.FC<AuthenticationGuardProps> = ({ component }) => {
    const Component = withAuthenticationRequired(component, {
        onRedirecting: () => (
            <div className="page-layout">
                <PageLoader />
            </div>
        ),
    });

    return <Component />;
};
