import { useAuth0 } from "@auth0/auth0-react";
import { NavLink } from "react-router-dom";
import LoginButton from "./LoginButton";
import LogoutButton from "./LogoutButton";
import { SignupButton } from "./SignupButton";

const NavBarBrand = () => {
    return (
        <div className="nav-bar__brand">
            <NavLink to="/">
                <img
                    className="nav-bar__logo"
                    src="https://cdn.auth0.com/blog/hub/code-samples/hello-world/auth0-logo.svg"
                    alt="Auth0 shield logo"
                    width="122"
                    height="36"
                />
            </NavLink>
        </div>
    );
};

type NavBarTabProps = {
    path: string,
    label: string,
};

const NavBarTab: React.FC<NavBarTabProps> = ({ path, label }) => {
    return (
        <NavLink
            to={path}
            end
            className={({ isActive }) =>
                "nav-bar__tab " + (isActive ? "nav-bar__tab--active" : "")
            }
        >
            {label}
        </NavLink>
    );
};

const NavBarTabs = () => {
    const { isAuthenticated } = useAuth0();

    return (
        <div className="nav-bar__tabs">
            <NavBarTab path="/profile" label="Profile" />
            <NavBarTab path="/public" label="Public" />
            {isAuthenticated && (
                <>
                    <NavBarTab path="/protected" label="Protected" />
                    <NavBarTab path="/admin" label="Admin" />
                </>
            )}
        </div>
    );
};

const NavBarButtons = () => {
    const { isAuthenticated } = useAuth0();

    return (
        <div className="nav-bar__buttons">
            {!isAuthenticated && (
                <>
                    <SignupButton />
                    <LoginButton />
                </>
            )}
            {isAuthenticated && (
                <>
                    <LogoutButton />
                </>
            )}
        </div>
    );
};

export const NavBar = () => {
    return (
        <div className="nav-bar__container">
            <nav className="nav-bar">
                <NavBarBrand />
                <NavBarTabs />
                <NavBarButtons />
            </nav>
        </div>
    );
};
