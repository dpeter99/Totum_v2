import {useService} from "@/lib/app-host";
import { host } from "@/App.tsx";
import { Outlet } from "react-router-dom";
import {Header} from "@/lumin/components/Header/Header.tsx";
import {Shell} from "@/lumin/components/Shell/Shell.tsx";
import {AppLogo} from "@/components/AppLogo";
import {AvatarIcon} from "@/lumin/components/AvatarIcon";
import {HeaderSection} from "@/lumin/components/Header";

export const Layout = () => {
  const auth = useService(host, 'UserService');

  return (
    <Shell>
      <Header>
        <AppLogo/>
        {/*<TotumSwitcher/>*/}
        <HeaderSection>
          <AvatarIcon img={auth.profileImageUrl}/>
          <button onClick={() => void auth.Logout()}>Log out</button>
        </HeaderSection>
      </Header>
      <main>
        <Outlet/>
      </main>
    </Shell>
  )
}

//