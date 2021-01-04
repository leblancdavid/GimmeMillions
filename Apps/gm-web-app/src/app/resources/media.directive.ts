import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';

@Directive({
  selector: '[gmMedia]'
})
export class MediaDirective {

  @Input() set gmMedia(query: string) {
    // cleanup old listener
    if (this.removeListener) {
      this.removeListener();
    }
    this.setListener(query);
  }
  private hasView = false;
  private removeListener: (() => void) | undefined;

  constructor(
    private readonly viewContainer: ViewContainerRef,
    private readonly template: TemplateRef<any>
  ) { }

  private setListener(query: string) {
    const mediaQueryList = window.matchMedia(query);
    const listener = (event: any) => {
      // create view if true and not created already
      if (event.matches && !this.hasView) {
        this.hasView = true;
        this.viewContainer.createEmbeddedView(this.template);
      }
      // destroy view if false and created
      if (!event.matches && this.hasView) {
        this.hasView = false;
        this.viewContainer.clear();
      }
    };
    // run once and then add listener
    listener(mediaQueryList);
    mediaQueryList.addEventListener('change', listener);
    // add cleanup listener
    this.removeListener = () => mediaQueryList.removeEventListener('change', listener);
  }

}
