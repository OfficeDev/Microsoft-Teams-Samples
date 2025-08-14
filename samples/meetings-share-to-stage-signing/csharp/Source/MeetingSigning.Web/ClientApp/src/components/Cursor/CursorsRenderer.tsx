import { LivePresenceUser } from '@microsoft/live-share';
import { CursorLocationEvent } from 'hooks';
import { Cursor } from './Cursor';

export interface CursorsRendererProps {
  cursors: LivePresenceUser<CursorLocationEvent>[];
  parentBoundingBox: DOMRect | undefined;
}

/**
 * Takes an array of Cursor locations and renders them on a div.
 *
 * @param cursors An array of cursor locations events, which containers the user's
 * @param parentBoundingBox The DOM details of the bounding box where the cursors will be rendered.
 * This is used to ensure that cursors are rendered at the correct offset from the bounding box.
 *
 * @returns Cursors rendered over the parent div
 */
export function CursorsRenderer({
  cursors,
  parentBoundingBox,
}: CursorsRendererProps) {
  return (
    <>
      {cursors
        .map((p) => {
          let X = p.data?.X ?? 0;
          let Y = p.data?.Y ?? 0;

          if (parentBoundingBox) {
            // If the cursor is outside the bounds of the document, limit the rendering to the document.
            X = Math.min(X, parentBoundingBox.width) + parentBoundingBox.left;
            Y = Y + parentBoundingBox.top;
          }

          return <Cursor
            key={p.userId}
            X={X}
            Y={Y}
            displayName={p.data?.displayName ?? p.userId}
          />
        })}
    </>
  );
}
