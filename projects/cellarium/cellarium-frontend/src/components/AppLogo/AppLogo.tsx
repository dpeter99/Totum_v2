import {FC} from "react";
import Icon from "@mdi/react";
import {mdiPotSteam} from "@mdi/js";

import s from "./AppLogo.module.scss";

export const AppLogo: FC = () => {
  return (
    <div className={s.appLogo}>
      <Icon path={mdiPotSteam} size={'100%'} />
      Cellarium
    </div>
  )
}