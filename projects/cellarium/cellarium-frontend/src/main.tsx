import {ComponentPreviews, useInitial} from "@/dev";
import {DevSupport} from "@react-buddy/ide-toolbox";

import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import {Root} from './App.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <DevSupport
      ComponentPreviews={ComponentPreviews}
      useInitialHook={useInitial}
    >
      <Root />
    </DevSupport>
  </StrictMode>,
)
