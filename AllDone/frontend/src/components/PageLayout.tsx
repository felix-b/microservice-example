import { PropsWithChildren } from "react";
import { NavBar } from "./NavBar";
import { PageFooter } from "./PageFooter";

export const PageLayout: React.FC<PropsWithChildren> = ({ children }) => {
    return (
        <div className="page-layout">
            <NavBar />
            <div className="page-layout__content">{children}</div>
            <PageFooter />
        </div>
    );
};
