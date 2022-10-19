import { useEffect, useState } from 'react';
import { Button, Flex } from '@fluentui/react-northstar';
import styles from './StageControls.module.css';

export type StageControlsProps = {
  localUserInControl: boolean;
  localUserCanTakeControl: boolean;
  userInControl: boolean;
  nameOfUserInControl?: string;
  followSuspended: boolean;
  takeControl: () => void;
  clearControl: () => void;
  endSuspension: () => void;
};

/**
 * Component that adds buttons so a user can take control of a presentation, or follow the current presenter
 *
 * @param localUserInControl Boolean if the current user is in control
 * @param localUserCanTakeControl Boolean that says a user is allowed to take the control
 * @param userInControl Boolean that any user is in control
 * @param nameOfUserInControl The name of the user currently in control
 * @param followSuspended Boolean if the current user is no longer following the user in control
 * @param takeControl Function to take control of the presentation
 * @param clearControl Function to stop controlling of the presentation
 * @param endSuspension Function to end the follow suspension and to follow the current presenter again
 *
 * @returns The component
 */
export function StageControls({
  localUserInControl,
  localUserCanTakeControl,
  userInControl,
  nameOfUserInControl,
  followSuspended,
  takeControl,
  clearControl,
  endSuspension,
}: StageControlsProps) {
  return (
    <Flex className={styles.stageControls} vAlign="center">
      <>
        {localUserCanTakeControl ? (
          <Button
            content={
              localUserInControl
                ? 'Stop controlling'
                : followSuspended && userInControl
                  ? `Follow ${nameOfUserInControl}`
                  : 'Take control'
            }
            size="small"
            onClick={() => {
              localUserInControl
                ? clearControl()
                : followSuspended && userInControl
                  ? endSuspension()
                  : takeControl();
            }}
          />
        ) : (
          <Button
            content={
              userInControl
                ? followSuspended
                  ? `Follow ${nameOfUserInControl}`
                  : `Following ${nameOfUserInControl ?? ''}`
                : 'No document controller'
            }
            size="small"
            disabled={!followSuspended}
            onClick={() => {
              followSuspended && userInControl && endSuspension();
            }}
          />
        )}
      </>
      {/* If someone is unable to take control we show this text in a disabled 'Take control' button  */}
      {localUserCanTakeControl &&
        nameOfUserInControl &&
        userInControl &&
        !followSuspended &&
        !localUserInControl && (
          <span className={styles.userInControl}>
            {`Following: ${nameOfUserInControl}`}
          </span>
        )}
    </Flex>
  );
}
