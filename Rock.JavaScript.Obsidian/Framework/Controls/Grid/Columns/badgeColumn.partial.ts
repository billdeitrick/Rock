import { standardColumnProps } from "@Obsidian/Core/Controls/grid";
import { defineComponent, PropType, VNode } from "vue";
import BadgeCell from "../Cells/badgeCell.partial.obs";

export default defineComponent({
    props: {
        ...standardColumnProps,

        format: {
            type: Object as PropType<VNode>,
            default: BadgeCell
        },

        /**
         * The lookup table to use when applying a custom label type tag to
         * the badge. The key is the text value of the field. The value is
         * a standard label suffix such as `primary` or `danger`.
         */
        classSource: {
            type: Object as PropType<Record<string, string>>,
            required: false
        },

        /**
         * The lookup table to use when applying a custom background color to
         * the badge. The key is the text value of the field. The value is
         * a standard CSS color designation.
         */
        colorSource: {
            type: Object as PropType<Record<string, string>>,
            required: false
        }
    }
});
