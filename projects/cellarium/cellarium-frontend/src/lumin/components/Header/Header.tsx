import { FC } from "react";
import {ReactChildren} from "@/utils";

import styles from "./Header.module.scss";
import classNames from "classnames/bind";

const cx = classNames.bind(styles);

type HeaderProps = {} & ReactChildren

export const Header : FC<HeaderProps> = (props) => {
  return(
    <header className={cx(styles.lumixHeader)}>
      {props.children}
    </header>
  )
}