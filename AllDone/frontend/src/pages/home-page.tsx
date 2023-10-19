import { PageLayout } from "../components/PageLayout";

const HeroBanner = () => {
    const logo = "https://cdn.auth0.com/blog/developer-hub/react-logo.svg";

    return (
        <div className="hero-banner hero-banner--pink-yellow">
            <div className="hero-banner__logo">
                <img className="hero-banner__image" src={logo} alt="React logo" />
            </div>
            <h1 className="hero-banner__headline">Hello, React World!</h1>
            <p className="hero-banner__description">
                This is a sample application that demonstrates the authentication flow
                for React apps using <strong>Auth0</strong>.
            </p>
            <a
                id="code-sample-link"
                target="_blank"
                rel="noopener noreferrer"
                href="https://developer.auth0.com/resources/code-samples/spa/react/basic-authentication"
                className="button button--secondary"
            >
                Check out the React code sample →
            </a>
        </div>
    );
};

const Auth0Features = () => {
    const featuresList = [
        {
            title: "Identity Providers",
            description:
                "Auth0 supports social providers such as Google, Facebook, and Twitter, along with Enterprise providers such as Microsoft Office 365, Google Apps, and Azure. You can also use any OAuth 2.0 Authorization Server.",
            resourceUrl: "https://auth0.com/docs/connections",
            icon: "https://cdn.auth0.com/blog/hello-auth0/identity-providers-logo.svg",
        },
        {
            title: "Multi-Factor Authentication",
            description:
                "You can require your users to provide more than one piece of identifying information when logging in. MFA delivers one-time codes to your users via SMS, voice, email, WebAuthn, and push notifications.",
            resourceUrl: "https://auth0.com/docs/multifactor-authentication",
            icon: "https://cdn.auth0.com/blog/hello-auth0/mfa-logo.svg",
        },
        {
            title: "Attack Protection",
            description:
                "Auth0 can detect attacks and stop malicious attempts to access your application such as blocking traffic from certain IPs and displaying CAPTCHA. Auth0 supports the principle of layered protection in security that uses a variety of signals to detect and mitigate attacks.",
            resourceUrl: "https://auth0.com/docs/attack-protection",
            icon: "https://cdn.auth0.com/blog/hello-auth0/advanced-protection-logo.svg",
        },
        {
            title: "Serverless Extensibility",
            description:
                "Actions are functions that allow you to customize the behavior of Auth0. Each action is bound to a specific triggering event on the Auth0 platform. Auth0 invokes the custom code of these Actions when the corresponding triggering event is produced at runtime.",
            resourceUrl: "https://auth0.com/docs/actions",
            icon: "https://cdn.auth0.com/blog/hello-auth0/private-cloud-logo.svg",
        },
    ];

    return (
        <div className="auth0-features">
            <h2 className="auth0-features__title">Explore Auth0 Features</h2>
            <div className="auth0-features__grid">
                {featuresList.map((feature) => (
                    <Auth0Feature
                        key={feature.resourceUrl}
                        title={feature.title}
                        description={feature.description}
                        resourceUrl={feature.resourceUrl}
                        icon={feature.icon}
                    />
                ))}
            </div>
        </div>
    );
};

type Auth0FeatureProps = {
    title: string, 
    description: string, 
    resourceUrl: string, 
    icon: string
}

const Auth0Feature: React.FC<Auth0FeatureProps> = ({ title, description, resourceUrl, icon }) => (
    <a
        href={resourceUrl}
        className="auth0-feature"
        target="_blank"
        rel="noopener noreferrer"
    >
        <h3 className="auth0-feature__headline">
            <img
                className="auth0-feature__icon"
                src={icon}
                alt="external link icon"
            />
            {title}
        </h3>
        <p className="auth0-feature__description">{description}</p>
    </a>
);

export const HomePage = () => (
    <PageLayout>
        <HeroBanner />
        <Auth0Features />
    </PageLayout>
);