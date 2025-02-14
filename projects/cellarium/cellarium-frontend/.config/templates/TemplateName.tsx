import { FC } from 'react';
import styles from './TemplateName.module.scss';

interface TemplateNameProps {}

export const TemplateName: FC<TemplateNameProps> = () => {
  return (
    <div className={styles.TemplateName}>
      TemplateName Component
    </div>
  );
};
