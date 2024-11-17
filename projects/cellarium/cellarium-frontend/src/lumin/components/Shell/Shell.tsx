import { FC } from "react";
import {ReactChildren} from "@/utils";

import styles from "./Shell.module.scss";

export const Shell: FC<ReactChildren> = (props) => {
  return (
    <div className={styles.lumixShell}>
      {props.children}
    </div>
  )
}