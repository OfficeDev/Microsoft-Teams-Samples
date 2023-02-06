import classnames from 'classnames';
import styles from './Badge.module.css';

type BadgeProps = {
  content: string;
  size?: 'small' | 'medium';
  rectangular?: boolean;
  backgroundColor: string;
};

/**
 * A Badge with test included.
 *
 * @param content Text to show on the Badge
 * @param size Can be `medium` or `small`. Default is `medium`
 * @param rectangular If the badge is rectangular or rounded. Default is `false`
 * @param backgroundColor Background of the Badge.
 * @returns
 */
export function Badge({
  content,
  size = 'medium',
  rectangular = false,
  backgroundColor,
}: BadgeProps) {
  const badgeInlineStyles = { backgroundColor: backgroundColor };

  const badgeClasses = classnames(
    styles.badge,
    { [styles.small]: size === 'small' },
    { [styles.rectangular]: rectangular },
  );

  return (
    <div className={badgeClasses} style={badgeInlineStyles}>
      {content}
    </div>
  );
}
