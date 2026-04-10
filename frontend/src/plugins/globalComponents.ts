import { defineAsyncComponent } from "vue";

import { Accordion } from '@/components/ui/accordion';
import { Alert } from '@/components/ui/alert';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Breadcrumb } from '@/components/ui/breadcrumb';
import { Card } from '@/components/ui/card';
import { Checkbox } from '@/components/ui/checkbox';
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
  } from '@/components/ui/dialog'
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { RadioGroup } from '@/components/ui/radio-group';
import { Select } from '@/components/ui/select';
import { Separator } from '@/components/ui/separator';
import { Skeleton } from '@/components/ui/skeleton';
import { Table } from '@/components/ui/table';
import { Tabs } from '@/components/ui/tabs';
import { Textarea } from '@/components/ui/textarea';
import { Toaster } from '@/components/ui/toast';
import { Toggle } from '@/components/ui/toggle';
import { Tooltip } from '@/components/ui/tooltip';

import { Icon } from '@iconify/vue';


export default (app: any) => {
    app.component('Accordion', Accordion);
    app.component('Alert', Alert);
    app.component('Badge', Badge);
    app.component('Button', Button);
    app.component('Breadcrumb', Breadcrumb);
    app.component('Card', Card);
    app.component('Checkbox', Checkbox);
    // Dialog components
    app.component('Dialog', Dialog);
    app.component('DialogContent', DialogContent);
    app.component('DialogDescription', DialogDescription);
    app.component('DialogFooter', DialogFooter);
    app.component('DialogHeader', DialogHeader);
    app.component('DialogTitle', DialogTitle);
    app.component('DialogTrigger', DialogTrigger);
    // End of Dialog components
    app.component('Input', Input);
    app.component('Label', Label);
    app.component('RadioGroup', RadioGroup);
    app.component('Select', Select);
    app.component('Separator', Separator);
    app.component('Skeleton', Skeleton);
    app.component('Table', Table);
    app.component('Tabs', Tabs);
    app.component('Textarea', Textarea);
    app.component('Toaster', Toaster);
    app.component('Toggle', Toggle);
    app.component('Tooltip', Tooltip);

    app.component('Icon', Icon);
    app.component(
      "IconifyIcon",
      defineAsyncComponent(() => import("@/components/custom/IconifyIcon.vue"))
    );
}