import { CursorLocationEvent } from 'hooks';
import styles from './Cursor.module.css';

export function Cursor({ X, Y, displayName }: CursorLocationEvent) {
  return (
    <>
      <div className={styles.cursor} style={{ left: X, top: Y }}>
        <FluentCursor />
        <p className={styles.cursorText}>{displayName ?? 'Error'}</p>
      </div>
    </>
  );
}

function FluentCursor() {
  return (
    <svg
      width="21"
      height="24"
      viewBox="5 1 21 24"
      xmlns="http://www.w3.org/2000/svg"
    >
          <path
            d="M7.921,2.3C6.936,1.532 5.5,2.234 5.5,3.482L5.5,20.491C5.5,21.913 7.295,22.537 8.177,21.421L12.367,16.121C12.68,15.726 13.158,15.495 13.662,15.495L20.514,15.495C21.942,15.495 22.563,13.687 21.435,12.811L7.921,2.299L7.921,2.3Z"
            fill="#A33D2A"
            stroke="#ffffff"
          />
    </svg>
  );
}
