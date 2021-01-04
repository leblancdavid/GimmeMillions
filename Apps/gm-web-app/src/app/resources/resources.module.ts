import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MediaDirective } from './media.directive';

@NgModule({
  declarations: [
    MediaDirective
  ],
  imports: [
    CommonModule
  ],
  exports: [
    MediaDirective
  ]
})
export class ResourcesModule { }
