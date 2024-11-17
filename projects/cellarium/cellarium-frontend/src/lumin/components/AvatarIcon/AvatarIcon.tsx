import {FC, MouseEventHandler} from "react";

import s from './AvatarIcon.module.scss';

export type AvatarIconProps = {
  img: string;
  onClick?: MouseEventHandler<HTMLButtonElement>
}

export const AvatarIcon: FC<AvatarIconProps> = (props) => {
  return (
    <button className={s.avatarIcon} onClick={props.onClick}>
      <img src={props.img} alt=""/>
    </button>
  )
}