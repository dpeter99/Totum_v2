import { UserManagerSettings, WebStorageStateStore } from "oidc-client-ts";


const origin = window.location.origin;

// custom settings that work with our ouwn OAuth server
const arachneSettings: UserManagerSettings = {
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-expect-error 
    authority: window['services__Arachne__https__0'],
    client_id: 'react-client',
    client_secret: '901564A5-E7FE-42CB-B10D-61EF6A8F3654',
    redirect_uri: `${origin}/oauth/callback`,
    silent_redirect_uri: `${origin}/oauth/callback`,
    post_logout_redirect_uri: `${origin}`,
    response_type: 'code',
    // this is for getting user.profile data, when open id connect is implemented
    //scope: 'api1 openid profile'
    // this is just for OAuth2 flow
    scope: 'api1 openid profile',
    loadUserInfo: true,
    
    stateStore: new WebStorageStateStore({ store: window.localStorage }),
    userStore: new WebStorageStateStore({ store: window.localStorage }),
};

export const arachneConfig = {
    settings: arachneSettings,
    flow: 'arachne'
};