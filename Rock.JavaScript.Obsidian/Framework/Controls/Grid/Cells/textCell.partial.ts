import { standardCellProps } from "@Obsidian/Core/Controls/grid";
import { defineComponent } from "vue";

export default defineComponent({
    props: {
        ...standardCellProps
    },

    setup(props) {
        return () => props.column.field ? props.row[props.column.field] : "";
    }
});
