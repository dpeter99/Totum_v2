import {ReactChildren} from "@/utils";
import {FC} from "react";

import s from '../Header.module.scss';

export const HeaderSection: FC<ReactChildren> = (props) => {
  return (
    <div className={s.headerSection}>
      {props.children}
    </div>
  );
};