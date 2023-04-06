import { standardColumnProps } from "@Obsidian/Core/Controls/grid";
import { defineComponent, PropType, VNode } from "vue";
import SelectCell from "../Cells/selectCell.partial.obs";
import SelectHeaderCell from "../Cells/selectHeaderCell.partial.obs";

export default defineComponent({
    props: {
        ...standardColumnProps,

        name: {
            type: String as PropType<string>,
            default: "__select"
        },

        format: {
            type: Object as PropType<VNode>,
            default: SelectCell
        },

        headerTemplate: {
            type: Object as PropType<VNode>,
            default: SelectHeaderCell
        },

        headerClass: {
            type: String as PropType<string>,
            default: "grid-select-field"
        },

        itemClass: {
            type: String as PropType<string>,
            default: "grid-select-field"
        }
    }
});
