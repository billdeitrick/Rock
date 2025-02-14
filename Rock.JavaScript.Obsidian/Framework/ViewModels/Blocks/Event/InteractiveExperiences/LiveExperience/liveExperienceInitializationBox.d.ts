//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

import { ExperienceStyleBag } from "@Obsidian/ViewModels/Event/InteractiveExperiences/experienceStyleBag";

/**
 * Class LiveExperienceInitializationBox.
 * Implements the Rock.ViewModels.Blocks.BlockBox
 */
export type LiveExperienceInitializationBox = {
    /**
     * Gets or sets the error message. A non-empty value indicates that
     * an error is preventing the block from being displayed.
     */
    errorMessage?: string | null;

    /** Gets or sets the experience token used to authenticate to the RealTime topic. */
    experienceToken?: string | null;

    /** Gets or sets a value indicating whether this experience is inactive. */
    isExperienceInactive: boolean;

    /** Gets or sets the keep alive interval in seconds. */
    keepAliveInterval: number;

    /** Gets or sets the navigation urls. */
    navigationUrls?: Record<string, string> | null;

    /** Gets or sets the security grant token. */
    securityGrantToken?: string | null;

    /** Gets or sets the style information for this experience. */
    style?: ExperienceStyleBag | null;
};
