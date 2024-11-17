import { FC } from 'react';
import styles from './Menu.module.scss';

interface MenuProps {}

const Menu: FC<MenuProps> = () => (
  <div className={styles.menu}>
    Menu Component
  </div>
);

export default Menu;
