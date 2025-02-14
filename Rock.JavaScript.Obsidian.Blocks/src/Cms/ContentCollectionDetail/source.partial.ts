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

import { computed, defineComponent,  PropType } from "vue";
import RockButton from "@Obsidian/Controls/rockButton";
import { ContentSourceBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionDetail/contentSourceBag";

/**
 * Calculates the brightness of the given CSS color string.
 * 
 * @param color The CSS color string to be calculated.
 * 
 * @returns A number between 0 and 1, or undefined if the brightness could not be determined.
 */
function calculateColorBrightness(color: string | undefined | null): number | undefined {
    if (!color) {
        return undefined;
    }

    // Create a node that we will use to parse the color specified into a
    // standard format we can use.
    const node = document.createElement("div");
    try {
        // Set the color and mark it to never display. Then add it to the body
        // so we can compute the style.
        node.setAttribute("style", `color: ${color} !important; display: none !important;`);
        document.body.appendChild(node);

        // Compute the color style. This always returns in one of two formats
        // no matter what the input format above was:
        // rgb(r,g,b)
        // rgba(r,g,b,a)
        const computedColor = window.getComputedStyle(node).color;
        const rgbaMatch = computedColor.match(/rgba?\((.*)\)/);
        if (!rgbaMatch) {
            return undefined;
        }

        const rgba = rgbaMatch[1].split(",").map(Number);

        // Calculate the brightness of the color.
        const brightness = Math.round(((rgba[0] * 299) + (rgba[1] * 587) + (rgba[2] * 114)) / 1000);

        // Our brightness is 0-255, make it 0-1.
        return Math.min(255, brightness) / 255;
    }
    finally {
        node.remove();
    }
}

export default defineComponent({
    name: "Cms.ContentCollectionDetail.Source",

    components: {
        RockButton
    },

    props: {
        modelValue: {
            type: Object as PropType<ContentSourceBag>,
            required: true
        }
    },

    emits: {
        "edit": (_value: ContentSourceBag) => true,
        "delete": (_value: ContentSourceBag) => true
    },

    setup(props, { emit }) {
        // #region Computed Values

        const barStyle = computed((): Record<string, string> => {
            return {
                backgroundColor: props.modelValue.color || "#eeeeee"
            };
        });

        const iconStyle = computed((): Record<string, string> => {
            return {
                backgroundColor: props.modelValue.color || "#eeeeee",
                color: (calculateColorBrightness(props.modelValue.color || "#eeeeee") ?? 0) > 0.5 ? "black" : "white"
            };
        });

        const iconCssClass = computed((): string => {
            return props.modelValue.iconCssClass ?? "";
        });

        const name = computed((): string => {
            return props.modelValue.name ?? "";
        });

        const includes = computed((): string => {
            if (!props.modelValue.attributes || props.modelValue.attributes.length === 0) {
                return "";
            }

            const attributeNames = props.modelValue.attributes.map(a => a.text ?? "");

            return `Includes: ${attributeNames.join(", ")}`;
        });

        const itemCount = computed((): number => {
            return props.modelValue.itemCount;
        });

        // #endregion

        // #region Event Handlers

        const onEditClick = (): void => {
            emit("edit", props.modelValue);
        };

        const onDeleteClick = (): void => {
            emit("delete", props.modelValue);
        };

        // #endregion

        return {
            barStyle,
            iconCssClass,
            iconStyle,
            includes,
            itemCount,
            name,
            onDeleteClick,
            onEditClick
        };
    },

    template: `
<div class="collection-source">
    <div class="bar" :style="barStyle"></div>
    <div class="icon" :style="iconStyle">
        <i v-if="iconCssClass" :class="iconCssClass"></i>
    </div>
    <div class="title">
        <div class="text">{{ name }}</div>
        <div v-if="includes" class="secondary-text">{{ includes }}</div>
    </div>
    <div class="actions">
        <span class="item-count badge badge-default">{{ itemCount }}</span>
        <span class="reorder-handle btn btn-default btn-sm"><i class="fa fa-bars"></i></span>
        <RockButton btnSize="sm" @click="onEditClick"><i class="fa fa-pencil"></i></RockButton>
        <RockButton btnSize="sm" @click="onDeleteClick"><i class="fa fa-times"></i></RockButton>
    </div>
</div>
`
});
